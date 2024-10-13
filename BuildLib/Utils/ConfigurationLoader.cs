using System.Runtime.InteropServices;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BuildLib.FileSystem;
using BuildLib.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Serilog;

namespace BuildLib.Utils;

public class ConfigurationLoader(HostApplicationBuilder builder)
{
    public int Tries = 0;

    public async Task LoadAppSecretsJson()
    {
        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") is not "Development")
        {
            Log.Information("Environment is not Development, skipping secrets json load");
            return;
        }

        await using var provider = builder.Services.BuildServiceProvider();
        var dirService = provider.GetRequiredService<DirectoryService>();
        var solution = dirService.GetSolution();
        var project = solution.GetProject("BuildLib").NotNull();
        var secretsId = project.GetProperty("UserSecretsId");
        if (secretsId is null)
        {
            throw new InvalidOperationException("UserSecretsId was null, cannot load secrets json");
        }

        builder.Configuration.AddUserSecrets<Container>();

        var courseConfiguration = new CoursePlannerConfiguration();
        builder.Configuration.Bind(courseConfiguration);
        if (courseConfiguration.Validate() is { } exc)
        {
            Log.Information("Missing secrets, attempting to populate secrets json: {Message}", exc.Message);
        }

        var keyUri = courseConfiguration.KeyUri;

        if (keyUri is null)
        {
            throw new InvalidOperationException("KeyUri was null, cannot load secrets json");
        }

        var client = GetSecretClient(keyUri);

        var secrets = await client.GetPropertiesOfSecretsAsync().Select(x => client.GetSecret(x.Name)).ToListAsync();
        var connectionString = secrets.First(x => x.Value.Name == "ConnectionString").Value.Value;
        var configs = await new ConfigurationClient(connectionString)
            .GetConfigurationSettingsAsync(new SettingSelector()
                { Fields = SettingFields.Key | SettingFields.Value }
            )
            .ToListAsync();


        var json = secrets.ToDictionary(x => x.Value.Name, x => x.Value.Value);
        foreach (var config in configs)
        {
            json[config.Key] = config.Value;
        }

        json = json.ToDictionary(x => x.Key.Replace("--", ":"), x => x.Value);


        string jsonPath;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            jsonPath = $"~/.microsoft/usersecrets/{secretsId}/secrets.json";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            jsonPath = $@"%APPDATA%\Microsoft\UserSecrets\{secretsId}\secrets.json";
        }
        else
        {
            throw new PlatformNotSupportedException($"Platform not supported {RuntimeInformation.OSDescription}");
        }

        var jsonPathResolved = Environment.ExpandEnvironmentVariables(jsonPath);
        var jsonPathResolvedInfo = new FileInfo(jsonPathResolved);
        await File.WriteAllTextAsync(jsonPathResolvedInfo.FullName,
            JsonConvert.SerializeObject(json, Formatting.Indented)
        );
        Log.Information("Wrote secrets json to {Path}", jsonPathResolvedInfo.FullName);
    }

    private static SecretClient GetSecretClient(string keyUri)
    {
        var secretClient = new SecretClient(new(new(keyUri)),
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeVisualStudioCredential = true,
                    ExcludeEnvironmentCredential = true,
                    ExcludeVisualStudioCodeCredential = true,
                    ExcludeSharedTokenCacheCredential = true,
                    ExcludeAzurePowerShellCredential = true,
                    ExcludeInteractiveBrowserCredential = true,
                    ExcludeManagedIdentityCredential = true,
                    ExcludeWorkloadIdentityCredential = true,
                    ExcludeAzureDeveloperCliCredential = true,
                }
            )
        );
        return secretClient;
    }
}
using System.Reflection;
using System.Runtime.InteropServices;
using Azure.Data.AppConfiguration;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BuildLib.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace BuildLib.Utils;

public class RemoteConfigurationClient(SecretClient client)
{
    public Dictionary<string, string> GetRemoteConfigurations()
    {
        var secrets = client
            .GetPropertiesOfSecrets()
            .Select(x => client.GetSecret(x.Name))
            .ToDictionary(x => x.Value.Name, x => x.Value.Value);

        var configs = new ConfigurationClient(secrets[nameof(CoursePlannerConfiguration.ConnectionString)])
            .GetConfigurationSettings(new SettingSelector
                { Fields = SettingFields.Key | SettingFields.Value }
            );


        foreach (var config in configs)
        {
            secrets[config.Key] = config.Value;
        }

        return secrets.ToDictionary(x => x.Key.Replace("--", ":"), x => x.Value);
    }
}

public class ConfigurationLoader(HostApplicationBuilder builder) : IDisposable
{
    private ServiceProvider? _serviceProviderInstance;
    private ServiceProvider Provider => _serviceProviderInstance ??= builder.Services.BuildServiceProvider();

    public void Dispose()
    {
        _serviceProviderInstance?.Dispose();
    }

    public void LoadAppConfigsStandard()
    {
        var secretClient = Provider.GetRequiredService<SecretClient>();
        var connString = secretClient.GetSecret(nameof(CoursePlannerConfiguration.ConnectionString)).Value.Value;


        builder
            .Configuration
            .AddAzureKeyVault(secretClient, new KeyVaultSecretManager())
            .AddAzureAppConfiguration(options =>
                options.Connect(connString).ConfigureKeyVault(s => s.Register(secretClient))
            );
    }

    private static string GetUserSecretsId()
    {
        var assembly = Assembly.GetAssembly(typeof(Container));
        if (assembly is null)
        {
            throw new InvalidOperationException("Assembly was null");
        }

        var attribute = assembly.GetCustomAttribute<UserSecretsIdAttribute>() ??
                        throw new InvalidOperationException("UserSecretsIdAttribute was null");
        return attribute.UserSecretsId;
    }

    public void LoadAppConfigs()
    {
        var environment = builder.Configuration.GetValue<string>("DOTNET_ENVIRONMENT");
        if (environment is not "Development")
        {
            Log.Information(
                "Environment is not Development, skipping secrets json load. Loading from azure credentials {Environment}",
                environment
            );
            LoadAppConfigsStandard();
            return;
        }

        builder.Configuration.AddUserSecrets<Container>();

        if (!NeedsRemoteSecrets())
        {
            return;
        }

        var remoteClient = Provider.GetRequiredService<RemoteConfigurationClient>();
        var dict = remoteClient.GetRemoteConfigurations();
        var jsonFile = GetSecretJsonFile();
        WriteJsonFile();
        // add a second time to load new secrets
        builder.Configuration.AddUserSecrets<Container>();
        return;

        bool NeedsRemoteSecrets()
        {
            var courseConfiguration = new CoursePlannerConfiguration();
            builder.Configuration.Bind(courseConfiguration);
            if (courseConfiguration.Validate() is not { } exc)
            {
                Log.Information("Secrets are valid, skipping secrets json load");
                return false;
            }

            Log.Information("Missing secrets, attempting to populate secrets json: {Message}", exc.Message);
            return true;
        }

        FileInfo GetSecretJsonFile()
        {
            var jsonPath = GetJsonPath();


            var jsonPathResolved = Environment.ExpandEnvironmentVariables(jsonPath);
            var jsonPathResolvedInfo = new FileInfo(jsonPathResolved);
            return jsonPathResolvedInfo;

            string GetJsonPath()
            {
                var secretsId = GetUserSecretsId();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return $"~/.microsoft/usersecrets/{secretsId}/secrets.json";
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $@"%APPDATA%\Microsoft\UserSecrets\{secretsId}\secrets.json";
                }

                throw new PlatformNotSupportedException($"Platform not supported {RuntimeInformation.OSDescription}");
            }
        }

        void WriteJsonFile()
        {
            File.WriteAllText(jsonFile.FullName,
                JsonConvert.SerializeObject(dict, Formatting.Indented)
            );
            Log.Information("Wrote secrets json to {Path}", jsonFile.FullName);
        }
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
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using BuildLib.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildLib.Utils;

// [Inject(Lifetime = ServiceLifetime.Singleton)]
// public class RemoteConfigurationClient(SecretClient client)
// {
//     public Dictionary<string, string> GetRemoteConfigurations()
//     {
//         var secrets = client
//             .GetPropertiesOfSecrets()
//             .Select(x => client.GetSecret(x.Name))
//             .ToDictionary(x => x.Value.Name, x => x.Value.Value);
//
//         var configs = new ConfigurationClient(secrets[nameof(AppConfiguration.ConnectionString)])
//             .GetConfigurationSettings(new SettingSelector
//                 { Fields = SettingFields.Key | SettingFields.Value }
//             );
//
//         // var configs =
//         //     new ConfigurationClient(Constants.ConnectionStringKey)
//         //         .GetConfigurationSettings(new SettingSelector
//         //             { Fields = SettingFields.Key | SettingFields.Value }
//         //         );
//
//         var mergedConfigs = new Dictionary<string, string>();
//
//         foreach (var config in configs)
//         foreach (var secret in secrets)
//         {
//             secrets[config.Key] = config.Value;
//             mergedConfigs[secret.Key] = secret.Value;
//         }
//
//         return secrets.ToDictionary(x => x.Key.Replace("--", ":"), x => x.Value);
//
//
//
//     }
// }

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
        // var connString = secretClient.GetSecret(Constants.ConnectionStringKey).Value.Value;


        builder
            .Configuration
            .AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        // .AddAzureAppConfiguration(options =>
        //     options.Connect(connString).ConfigureKeyVault(s => s.Register(secretClient))
        // );
    }


    public void LoadAppConfigs()
    {
        var log = Log.ForContext<ConfigurationLoader>();
        var environment = builder.Configuration.GetValue<string>("DOTNET_ENVIRONMENT");
        if (environment is not "Development")
        {
            log.Information(
                "Environment is not Development, skipping secrets json load. Loading from azure credentials {Environment}",
                environment
            );
            LoadAppConfigsStandard();
            return;
        }

        builder.Configuration.AddUserSecrets<Container>(true, true);

        if (!NeedsRemoteSecrets())
        {
            return;
        }

        LoadAppConfigsStandard();

        var config = builder.Configuration.Get<AppConfiguration>() ??
                     throw new InvalidOperationException("AppConfiguration is null");
        config.ValidateOrThrow();
        var manager = Provider.GetRequiredService<UserSecretManager<Container>>();
        manager.Save(config);


        builder.Configuration.AddUserSecrets<Container>(true, true);
        return;

        bool NeedsRemoteSecrets()
        {
            var courseConfiguration = new AppConfiguration();
            builder.Configuration.Bind(courseConfiguration);
            if (courseConfiguration.Validate() is not { } exc)
            {
                log.Information("Secrets are valid, skipping secrets json load");
                return false;
            }

            log.Information("Missing secrets, attempting to populate secrets json: {Message}", exc.Message);
            return true;
        }
    }
}
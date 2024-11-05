using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using BuildLib.AndroidPublish;
using BuildLib.CloudServices;
using BuildLib.FileSystem;
using BuildLib.Secrets;
using BuildLib.SolutionBuild;
using CaseConverter;
using FluentValidation;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.Utils;

public class ReleaseProject
{
    public required Project Value { get; init; }
}

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder AddServices(this HostApplicationBuilder builder)
    {
        builder
            .Services
            .AddInjectables()
            .AddValidatorsFromAssemblyContaining<FileNameValidator>(ServiceLifetime.Transient)
            .AddSingleton<Solution>(x => x.GetRequiredService<DirectoryService>().GetSolution())
            .AddSingleton<ReleaseProject>(p =>
                {
                    var solution = p.GetRequiredService<Solution>();
                    var projectName =
                        p.GetAppConfigurationOrThrow(x => x.GooglePlayDeveloperApiConfiguration.ProjectName);
                    var project = solution.GetProjectWithValidation(projectName);
                    return new ReleaseProject { Value = project };
                }
            )
            .AddSingleton<IMsBuildProject>(p => p.GetRequiredService<MsBuildService>().GetMsBuildProject())
            .AddSingleton<AndroidSigningKeyStoreOptions>(p =>
                {
                    var c = p.GetAppConfigurationOrThrow(x => x.DotnetPublishAndroidConfiguration);
                    return new AndroidSigningKeyStoreOptions
                    {
                        AndroidSigningKeyAlias = c.AndroidSigningKeyAlias,
                        AndroidSigningKeyStore = Path.GetFullPath(c.AndroidSigningKeyStore),
                        AndroidSigningKeyPass = c.AndroidSigningKeyPass,
                        AndroidSigningStorePass = c.AndroidSigningKeyPass
                    }.ToValidatedAndroidSigningKeyStoreOptions();
                }
            )
            .AddSingleton<DotNetPublishSettings>(x => x.GetRequiredService<DotNetPublishSettingsFactory>().Create())
            .AddSingleton<AndroidPublisherService>(p =>
                {
                    var initializer = p.GetRequiredService<BaseClientService.Initializer>();
                    return new AndroidPublisherService(initializer);
                }
            );
        return builder;
    }

    public static HostApplicationBuilder AddPascalCaseKeys(this HostApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var pascalCollection = config
            .AsEnumerable()
            .SelectKeys(x => x.Key.ToPascalCase())
            .ExceptBy(config.AsEnumerable().Select(x => x.Key), x => x.Key);


        config.AddInMemoryCollection(pascalCollection);

        return builder;
    }

    public static HostApplicationBuilder AddCloudServices(this HostApplicationBuilder builder)
    {
        builder
            .Services.AddSingleton<BaseClientService.Initializer>(c =>
                c.GetRequiredService<InitializerFactory>().Create()
            )
            .AddSingleton<BlobServiceClient>(p =>
                new BlobServiceClient(p.GetAppConfigurationOrThrow(x => x.AzureBlobStorageConfiguration.ConnectionString
                    )
                )
            )
            .AddSingleton<BlobContainerClient>(p =>
                p
                    .GetRequiredService<BlobServiceClient>()
                    .GetBlobContainerClient(
                        p.GetAppConfigurationOrThrow(x => x.AzureBlobStorageConfiguration.ContainerName)
                    )
            )
            .AddSingleton<SecretClient>(p =>
                {
                    var keyUri = p.GetAppConfigurationOrThrow(x => x.AzureKeyVaultConfiguration.Uri);
                    return new SecretClient(
                        new(keyUri),
                        new AzureCliCredential(new AzureCliCredentialOptions
                            { Diagnostics = { IsLoggingEnabled = true, } }
                        )
                    );
                }
            );
        return builder;
    }

    public static HostApplicationBuilder BindConfiguration(this HostApplicationBuilder builder)
    {
        var services = builder.Services;

        services
            .AddOptions<AppConfiguration>()
            .Bind(builder.Configuration);

        builder
            .Configuration.GetConfigurationOrThrow<AppConfiguration>()
            .ValidateOrThrow();

        var googleClientSectionKey = NamespaceData
            .FromNameofExpression(nameof(AppConfiguration.GooglePlayDeveloperApiConfiguration.GoogleClientKey))
            .ConfigKey;

        services
            .AddOptions<GoogleClientKey>()
            .Bind(builder.Configuration.GetSection(googleClientSectionKey) ??
                  throw new NullReferenceException($"{nameof(GoogleClientKey)} was null")
            );

        services
            .AddOptions<DotnetPublishAndroidConfiguration>()
            .Bind(builder.Configuration.GetSection(nameof(AppConfiguration.DotnetPublishAndroidConfiguration)));

        services
            .AddOptions<AzureBlobStorageConfiguration>()
            .Bind(builder.Configuration.GetSection(nameof(AppConfiguration.AzureBlobStorageConfiguration)));

        services
            .AddOptions<AzureKeyVaultConfiguration>()
            .Bind(builder.Configuration.GetSection(nameof(AppConfiguration.AzureKeyVaultConfiguration)));

        services
            .AddOptions<GooglePlayDeveloperApiConfiguration>()
            .Bind(builder.Configuration.GetSection(nameof(AppConfiguration.GooglePlayDeveloperApiConfiguration)));

        return builder;
    }

    public static HostApplicationBuilder AddConfiguration(this HostApplicationBuilder builder)
    {
        using var loader = new ConfigurationLoader(builder);
        loader.LoadAppConfigs();
        return builder;
    }
}
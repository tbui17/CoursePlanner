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
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Semver;

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
                    var projectName = p.GetAppConfigurationOrThrow(x => x.ProjectName);
                    var project = solution.GetProjectWithValidation(projectName);
                    return new ReleaseProject { Value = project };
                }
            )
            .AddKeyedSingleton<SemVersion>(nameof(AppConfiguration.AppVersion),
                (p, _) =>
                {
                    var versionStr = p.GetAppConfigurationOrThrow(x => x.AppVersion);
                    return SemVersion.Parse(versionStr, SemVersionStyles.AllowV);
                }
            )
            .AddSingleton<IMsBuildProject>(p => p.GetRequiredService<MsBuildService>().GetMsBuildProject())
            .AddSingleton<AndroidSigningKeyStoreOptions>(p =>
                {
                    var c = p.GetAppConfigurationOrThrow();
                    return new AndroidSigningKeyStoreOptions
                    {
                        AndroidSigningKeyAlias = c.AndroidSigningKeyAlias,
                        AndroidSigningKeyStore = c.AndroidSigningKeyStore,
                        AndroidSigningKeyPass = c.Key,
                        AndroidSigningStorePass = c.Key
                    }.ToValidatedAndroidSigningKeyStoreOptions();
                }
            )
            .AddSingleton<DotNetPublishSettings>(x => x.GetRequiredService<DotNetPublishSettingsFactory>().Create());
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
                new BlobServiceClient(p.GetAppConfigurationOrThrow(x => x.BlobConnectionString))
            )
            .AddSingleton<BlobContainerClient>(p =>
                p
                    .GetRequiredService<BlobServiceClient>()
                    .GetBlobContainerClient(p.GetAppConfigurationOrThrow(x => x.BlobContainerName))
            )
            .AddSingleton<SecretClient>(p =>
                {
                    var keyUri = p.GetAppConfigurationOrThrow(x => x.KeyUri);
                    return new SecretClient(
                        new(keyUri),
                        new AzureCliCredential(new AzureCliCredentialOptions
                            { Diagnostics = { IsLoggingEnabled = true, } }
                        )
                    );
                }
            )
            .AddSingleton<RemoteConfigurationClient>();
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


        services
            .AddOptions<GoogleServiceAccount>()
            .Bind(builder.Configuration.GetSection(nameof(AppConfiguration.GoogleServiceAccount)) ??
                  throw new NullReferenceException($"{nameof(GoogleServiceAccount)} was null")
            );

        return builder;
    }

    public static HostApplicationBuilder AddConfiguration(this HostApplicationBuilder builder)
    {
        using var loader = new ConfigurationLoader(builder);
        loader.LoadAppConfigs();
        return builder;
    }
}
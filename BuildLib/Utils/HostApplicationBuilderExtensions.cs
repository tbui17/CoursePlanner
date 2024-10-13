using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using BuildLib.CloudServices;
using BuildLib.FileSystem;
using BuildLib.Secrets;
using CaseConverter;
using FluentValidation;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scrutor;

namespace BuildLib.Utils;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder AddServices(this HostApplicationBuilder builder)
    {
        builder
            .Services.Scan(scan => scan
                .FromCallingAssembly()
                .AddClasses(x => x.WithAttribute<InjectAttribute>())
                .UsingRegistrationStrategy(RegistrationStrategy.Throw)
                .AsSelf()
                .AsMatchingInterface()
                .WithLifetime(ServiceLifetime.Transient)
            )
            .AddValidatorsFromAssemblyContaining<FileNameValidator>(ServiceLifetime.Transient);
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
        builder
            .Services
            .AddOptions<CoursePlannerConfiguration>()
            .Bind(builder.Configuration)
            .Validate(conf =>
                {
                    conf.ValidateOrThrow();
                    return true;
                }
            )
            .ValidateOnStart();

        builder
            .Services
            .AddOptions<GoogleServiceAccount>()
            .Bind(builder.Configuration.GetSection(nameof(CoursePlannerConfiguration.GoogleServiceAccount)) ??
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
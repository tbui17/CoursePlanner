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
        builder.Services.Scan(scan => scan
            .FromCallingAssembly()
            .AddClasses(x => x.WithAttribute<InjectAttribute>())
            .UsingRegistrationStrategy(RegistrationStrategy.Throw)
            .AsSelf()
            .AsMatchingInterface()
            .WithLifetime(ServiceLifetime.Transient)
        );
        builder.Services.AddValidatorsFromAssemblyContaining<FileNameValidator>(ServiceLifetime.Transient);
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
                new BlobServiceClient(p.GetAppConfiguration(x => x.BlobConnectionString))
            )
            .AddSingleton<BlobContainerClient>(p =>
                p
                    .GetRequiredService<BlobServiceClient>()
                    .GetBlobContainerClient(p.GetAppConfiguration(x => x.BlobContainerName))
            );
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
        new ConfigurationLoader(builder).LoadAppSecretsJson().Wait();
        return builder;

        // var uri = builder.Configuration.GetConfiguration<string>(nameof(CoursePlannerConfiguration.KeyUri));
        // var secretClient = new SecretClient(new(new(uri)),
        //     new DefaultAzureCredential(new DefaultAzureCredentialOptions()
        //         {
        //             ExcludeVisualStudioCredential = true,
        //             ExcludeEnvironmentCredential = true,
        //             ExcludeVisualStudioCodeCredential = true,
        //             ExcludeSharedTokenCacheCredential = true,
        //             ExcludeAzurePowerShellCredential = true,
        //             ExcludeInteractiveBrowserCredential = true,
        //             ExcludeManagedIdentityCredential = true,
        //             ExcludeWorkloadIdentityCredential = true,
        //             ExcludeAzureDeveloperCliCredential = true,
        //         }
        //     )
        // );
        // var connectionString = secretClient.GetSecret(nameof(CoursePlannerConfiguration.ConnectionString)).Value.Value;
        // var config = builder.Configuration;
        //
        // config
        //     .AddAzureKeyVault(secretClient, new KeyVaultSecretManager())
        //     .AddAzureAppConfiguration(options =>
        //         options.Connect(connectionString).ConfigureKeyVault(s => s.Register(secretClient))
        //     )
        //     .AddUserSecrets<Container>();
        //
        // builder.Services.AddSingleton(secretClient);
        //
        // return builder;
    }
}
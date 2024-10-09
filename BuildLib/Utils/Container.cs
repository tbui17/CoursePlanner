using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using BuildLib.Clients;
using BuildLib.Secrets;
using CaseConverter;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scrutor;
using Serilog;

namespace BuildLib.Utils;

public class Container(IHost host)
{
    public T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

    public T GetConfiguration<T>() => host.Services.GetConfiguration<T>();

    public static Container Init<T>() where T : class
    {
        var builder = CreateBuilder<T>();

        var host = builder.Build();

        var container = new Container(host);

        return container;
    }

    public static HostApplicationBuilder CreateBuilder<T>() where T : class
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.Scan(scan => scan
            .FromCallingAssembly()
            .AddClasses(c => c.NotInNamespaceOf<CoursePlannerConfiguration>().WithoutAttribute<IgnoreAttribute>())
            .UsingRegistrationStrategy(RegistrationStrategy.Throw)
            .AsSelf()
            .WithLifetime(ServiceLifetime.Transient)
        );


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

        var uri = builder.Configuration.GetConfiguration<string>(nameof(CoursePlannerConfiguration.KeyUri));
        var secretClient = new SecretClient(new(new(uri)), new DefaultAzureCredential());
        var connectionString = secretClient.GetSecret(nameof(CoursePlannerConfiguration.ConnectionString)).Value.Value;
        var config = builder.Configuration;

        builder.Services.AddSingleton(secretClient);
        config.AddEnvironmentVariables();
        AddPascalCaseKeys(config);
        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
        config.AddAzureAppConfiguration(options => options.Connect(connectionString));
        config.AddUserSecrets<T>();


        builder
            .Services
            .AddOptions<CoursePlannerConfiguration>()
            .Bind(builder.Configuration)
            .Validate(conf =>
                {
                    conf.Validate();
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


        builder.Logging.AddSerilog();

        return builder;
    }

    private static void AddPascalCaseKeys(IConfigurationManager config)
    {
        var originalKeys = config
            .AsEnumerable()
            .Select(x => x.Key)
            .ToHashSet();

        var pascalCollection = config
            .AsEnumerable()
            .SelectKeys(x => x.Key.ToPascalCase())
            .Thru(kvp => kvp.Where(x => !originalKeys.Contains(x.Key)));


        config.AddInMemoryCollection(pascalCollection);
    }
}

public static class ProviderExtensions
{
    public static T GetConfiguration<T>(this IServiceProvider provider) =>
        provider.GetRequiredService<IConfiguration>().Get<T>() ??
        throw new ArgumentException($"{typeof(T)} was null.");

    public static T GetConfiguration<T>(this IConfiguration config) =>
        config.Get<T>() ??
        throw new ArgumentException($"{typeof(T)} was null.");

    public static T GetConfiguration<T>(this IConfiguration config, string name) => config.GetValue<T>(name) ??
        throw new ArgumentException($"{name} {typeof(T)} was null.");

    public static T GetConfiguration<T>(this IServiceProvider provider, string name) =>
        provider.GetRequiredService<IConfiguration>().GetConfiguration<T>(name);

    public static T GetAppConfiguration<T>(
        this IServiceProvider provider,
        Func<CoursePlannerConfiguration, T> selector
    ) =>
        selector(provider.GetAppConfiguration());

    public static CoursePlannerConfiguration GetAppConfiguration(
        this IServiceProvider provider
    ) =>
        provider.GetConfiguration<CoursePlannerConfiguration>();
}
using BuildLib.Clients;
using BuildLib.Secrets;
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

    public T GetConfig<T>() => host.Services.GetRequiredService<IConfiguration>().Get<T>() ??
                               throw new NullReferenceException("Configuration was null.");

    public static Container Init<T>() where T : class
    {
        var builder = CreateBuilder<T>();

        var host = builder.Build();


        return new Container(host);
    }

    public static HostApplicationBuilder CreateBuilder<T>() where T : class
    {
        var builder = Host.CreateApplicationBuilder();
        AddSecrets<T>(builder);
        builder.Services.Scan(scan => scan
            .FromCallingAssembly()
            .AddClasses(c => c.NotInNamespaceOf<CoursePlannerSecrets>().WithoutAttribute<IgnoreAttribute>())
            .UsingRegistrationStrategy(RegistrationStrategy.Throw)
            .AsSelf()
            .WithLifetime(ServiceLifetime.Transient)
        );


        builder.Services.AddSingleton<BaseClientService.Initializer>(c =>
            c.GetRequiredService<InitializerFactory>().Create()
        );

        // nameof(CoursePlannerSecrets.KeyUri)
        //     .Thru(builder.Configuration.GetValue<string>)
        //     .Thru(str => string.IsNullOrWhiteSpace(str) ? throw new ArgumentException("KeyUri is missing") : str)
        //     .Thru(str => new Uri(str))
        //     .Tap(uri => builder.Configuration.AddAzureKeyVault(uri, new DefaultAzureCredential()));

        builder.Logging.AddSerilog();

        return builder;
    }

    private static void AddSecrets<T>(IHostApplicationBuilder builder) where T : class
    {
        builder.Configuration.AddUserSecrets<T>();
        builder
            .Services
            .AddOptions<CoursePlannerSecrets>()
            .Bind(builder.Configuration);

        var secrets = builder.Configuration.Get<CoursePlannerSecrets>()!;
        var nulls = secrets
            .GetPropertiesRecursive()
            .Where(x => x.Value is string s
                ? string.IsNullOrWhiteSpace(s)
                : x.Value is null
            )
            .Select(x => x.GetPath())
            .Select(x => string.Join(".", x))
            .ToList();

        if (nulls.Count > 0)
        {
            var csv = string.Join(", ", nulls);
            throw new ArgumentException($"Secrets are missing: {csv}");
        }
    }
}
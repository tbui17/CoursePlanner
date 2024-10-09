using Azure.Identity;
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
        builder.Services.Scan(scan => scan
            .FromCallingAssembly()
            .AddClasses(c => c.NotInNamespaceOf<CoursePlannerConfiguration>().WithoutAttribute<IgnoreAttribute>())
            .UsingRegistrationStrategy(RegistrationStrategy.Throw)
            .AsSelf()
            .WithLifetime(ServiceLifetime.Transient)
        );


        builder.Services.AddSingleton<BaseClientService.Initializer>(c =>
            c.GetRequiredService<InitializerFactory>().Create()
        );


        new EnvVarSourceLoader().Load(builder.Configuration);
        builder.Configuration.AddEnvironmentVariables();
        builder.Configuration.AddUserSecrets<Container>();
        builder
            .Configuration.AsEnumerable()
            .SelectKeys(x => x.Key.ToPascalCase())
            .Thru(kvp =>
                {
                    var set = builder.Configuration.AsEnumerable().Select(x => x.Key).ToHashSet();
                    return kvp.Where(x => !set.Contains(x.Key));
                }
            )
            .Tap(x => builder.Configuration.AddInMemoryCollection(x));
        nameof(CoursePlannerConfiguration.KeyUri)
            .Thru(builder.Configuration.GetValue<string>)
            .Thru(str => string.IsNullOrWhiteSpace(str) ? throw new ArgumentException("KeyUri is missing") : str)
            .Thru(str => new Uri(str))
            .Tap(uri => builder.Configuration.AddAzureKeyVault(uri, new DefaultAzureCredential()));


        builder
            .Configuration
            .AddAzureAppConfiguration(options => options
                .Connect(Environment.GetEnvironmentVariable("ConnectionString"))
            );


        builder
            .Services
            .AddOptions<CoursePlannerConfiguration>()
            .Bind(builder.Configuration);


        builder.Logging.AddSerilog();

        return builder;
    }
}

file class EnvVarSourceLoader
{
    public Dictionary<string, string> GetSettings()
    {
        var dict = Environment.GetEnvironmentVariables();

        var dict2 = new Dictionary<string, string>();

        foreach (string key in dict.Keys)
        {
            var value = dict[key];
            if (value is not string s)
            {
                throw new ArgumentException("Value is not a string");
            }

            dict2.Add(key, s);
        }

        var dict3 = new Dictionary<string, string>();

        foreach (var (key, value) in dict2)
        {
            var variants = new[]
                { key.ToPascalCase() };

            foreach (var variant in variants)
            {
                dict3[variant] = value;
            }
        }

        return dict3;
    }

    public void Load(IConfigurationBuilder builder)
    {
        var dict = GetSettings();
        builder.AddInMemoryCollection(dict!);
        builder.AddEnvironmentVariables();
    }
}
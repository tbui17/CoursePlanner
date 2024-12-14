using BuildLib.SolutionBuild;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildLib.Utils;

public class Container(IHost host)
{
    public IServiceProvider Services => host.Services;
    public T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

    public static Container Init() => Init(_ => { });

    public static Container Init(Action<HostApplicationBuilder> configure)
    {
        var builder = CreateBuilder();
        configure(builder);
        var host = builder.Build();
        var container = new Container(host);
        container.Resolve<GoogleLoggerInitializer>().InitGoogleLogger();

        return container;
    }

    private static HostApplicationBuilder CreateBuilder()
    {
        MsBuildService.Initialize();
        var builder = Host.CreateApplicationBuilder();

        builder
            .AddServices()
            .AddCloudServices()
            .AddPascalCaseKeys()
            .AddConfiguration()
            .BindConfiguration();

        builder.Logging.AddSerilog();


        return builder;
    }
}
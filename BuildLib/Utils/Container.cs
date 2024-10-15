using BuildLib.SolutionBuild;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Host = Microsoft.Extensions.Hosting.Host;

namespace BuildLib.Utils;

public class Container(IHost host)
{
    public IServiceProvider Services => host.Services;
    public T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

    public T GetConfiguration<T>() => host.Services.GetConfigurationOrThrow<T>();

    public static Container Init()
    {
        var builder = CreateBuilder();

        var host = builder.Build();

        var container = new Container(host);

        return container;
    }

    public static HostApplicationBuilder CreateBuilder()
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
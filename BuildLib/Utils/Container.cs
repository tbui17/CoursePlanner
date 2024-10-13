using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Host = Microsoft.Extensions.Hosting.Host;

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
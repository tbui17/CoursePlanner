using BuildLib.SolutionBuild;
using Google;
using Google.Apis.Logging;
using Google.Apis.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Host = Microsoft.Extensions.Hosting.Host;
using ILogger = Google.Apis.Logging.ILogger;

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
        ApplicationContext.RegisterLogger(new GoogleLogger(LogLevel.Debug,
                SystemClock.Default,
                typeof(HostApplicationBuilder)
            )
        );

        return builder;
    }
}

public class GoogleLogger : BaseLogger
{
    public GoogleLogger(LogLevel minimumLogLevel, IClock clock, Type forType) : base(minimumLogLevel, clock, forType)
    {
    }


    protected override ILogger BuildNewLogger(Type type) =>
        new GoogleLogger(MinimumLogLevel, Clock, type);


    protected override void Log(LogLevel logLevel, string formattedMessage)
    {
        Serilog.Log.Logger.Write(logLevel.ToLogEventLevel(), "Google log: {FormattedMessage}", formattedMessage);
    }
}
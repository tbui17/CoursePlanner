using BuildLib.Logging;
using BuildLib.SolutionBuild;
using Google;
using Google.Apis.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using LogLevel = Google.Apis.Logging.LogLevel;


namespace BuildLib.Utils;

public class Container(IHost host)
{
    public IServiceProvider Services => host.Services;
    public T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

    public T GetConfiguration<T>() => host.Services.GetConfigurationOrThrow<T>();

    public static Container Init() => Init(_ => { });

    public static Container Init(Action<HostApplicationBuilder> configure)
    {
        var builder = CreateBuilder();
        configure(builder);
        var host = builder.Build();
        var container = new Container(host);
        InitGoogleLogger();

        return container;

        void InitGoogleLogger()
        {
            var initLogger = container.Resolve<ILogger<Container>>();
            using var _ = initLogger.MethodScope();
            var fac = container.Resolve<ILoggerFactory>();
            var googleLogger = new GoogleLogger(LogLevel.Debug, SystemClock.Default, typeof(Container))
            {
                LogImplementation = (level, fmt, contextType) =>
                {
                    fac
                        .CreateLogger(contextType)
                        .Log(level.ToLogLevel(), "Google log: {FormattedMessage}", fmt);
                }
            };


            try
            {
                ApplicationContext.RegisterLogger(googleLogger);
            }
            catch (InvalidOperationException e) when (
                e.Message.Contains("A logger was already registered with this context"))
            {
                initLogger.LogInformation(e, "Google Logger already registered");
            }
        }
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
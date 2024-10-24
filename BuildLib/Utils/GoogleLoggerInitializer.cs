using BuildLib.Logging;
using Google;
using Google.Apis.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LogLevel = Google.Apis.Logging.LogLevel;

namespace BuildLib.Utils;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class GoogleLoggerInitializer(ILogger<GoogleLoggerInitializer> logger, ILoggerFactory loggerFactory)
{
    public void InitGoogleLogger()
    {
        using var _ = logger.MethodScope();

        var googleLogger = new GoogleLogger(LogLevel.Debug, SystemClock.Default, typeof(Container))
        {
            LogImplementation = (level, fmt, contextType) =>
            {
                loggerFactory
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
            logger.LogInformation(e, "Google Logger already registered");
        }
    }
}
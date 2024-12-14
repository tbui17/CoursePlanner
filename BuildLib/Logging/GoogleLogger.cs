using BuildLib.Utils;
using Google.Apis.Logging;
using Google.Apis.Util;

namespace BuildLib.Logging;

public class GoogleLogger(LogLevel minimumLogLevel, IClock clock, Type forType)
    : BaseLogger(minimumLogLevel, clock, forType)
{
    private static readonly Action<LogLevel, string, Type> DefaultLogImplementation =
        (logLevel, formattedMessage, type) =>
        {
            Serilog
                .Log.ForContext(type)
                .Write(logLevel.ToLogEventLevel(), "Google log: {FormattedMessage}", formattedMessage);
        };

    public Action<LogLevel, string, Type> LogImplementation { get; init; } = DefaultLogImplementation;

    protected override ILogger BuildNewLogger(Type type) =>
        new GoogleLogger(MinimumLogLevel, Clock, type) { LogImplementation = LogImplementation };


    protected override void Log(LogLevel logLevel, string formattedMessage)
    {
        LogImplementation(logLevel, formattedMessage, LoggerForType);
    }
}
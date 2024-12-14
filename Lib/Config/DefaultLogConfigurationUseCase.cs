using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Display;

namespace Lib.Config;

public class DefaultLogConfigurationUseCase : ILoggingUseCase
{
    public const string LogTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static readonly FileSinkOptions FileSinkOptions = new()
    {
        Path = Path.Combine("logs", "logs.json"),
        RestrictedToMinimumLevel = LogEventLevel.Information,
        RollingInterval = RollingInterval.Infinite,
        RetainedFileCountLimit = 3,
        RollOnFileSizeLimit = true,
        FlushToDiskInterval = TimeSpan.FromSeconds(60),
        Buffered = false
    };

    public LoggerConfiguration Configuration { get; set; } = new();


    public void SetMinimumLogLevel()
    {
        Configuration.MinimumLevel
            .Information();
    }

    public void AddSinks()
    {
        WriteConsole();
    }

    public void AddEnrichments()
    {
        Configuration
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("FriendlyApplicationName", AppDomain.CurrentDomain.FriendlyName)
            .Enrich.With(new StackTraceEnricher());
    }

    public void AddLogFilters()
    {
    }

    public void WriteConsole()
    {
        Configuration.WriteTo.Console(new MessageTemplateTextFormatter(LogTemplate), LogEventLevel.Information);
    }
}
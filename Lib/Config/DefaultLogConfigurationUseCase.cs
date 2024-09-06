using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Lib.Config;

public class DefaultLogConfigurationUseCase : ILoggingUseCase
{
    public LoggerConfiguration Configuration { get; set; } = new();

    public const string LogTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";


    public void SetMinimumLogLevel()
    {
        Configuration.MinimumLevel
            .Information()
            .WriteTo.File(FileSinkOptions);
    }

    public void AddSinks()
    {
        Configuration.WriteTo.Console(LogEventLevel.Information, LogTemplate);
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

    public static readonly FileSinkOptions FileSinkOptions = new()
    {
        Path = Path.Combine("logs", "logs.json"),
        RestrictedToMinimumLevel = LogEventLevel.Information,
        RollingInterval = RollingInterval.Infinite,
        RetainedFileCountLimit = 3,
        RollOnFileSizeLimit = true,
        FlushToDiskInterval = TimeSpan.FromSeconds(1),
        Buffered = false
    };

    public void AddLogFilters()
    {
    }
}
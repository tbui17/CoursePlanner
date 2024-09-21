using System.Text;
using Lib.Config;
using Lib.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace BaseTestSetup;

internal sealed class LogConfigurationTestUseCase : ILoggingUseCase
{
    public LoggerConfiguration Configuration { get; set; } = new();

    private DefaultLogConfigurationUseCase Base { get; set; }

    public LogConfigurationTestUseCase()
    {
        Base = new DefaultLogConfigurationUseCase { Configuration = Configuration };
    }


    public void SetMinimumLogLevel()
    {
        Configuration.MinimumLevel.Debug();
    }

    public void AddSinks()
    {
        Base.AddSinks();
        Configuration
            .WriteTo.Debug(LogEventLevel.Information, DefaultLogConfigurationUseCase.LogTemplate)
            .WriteTo.Console(LogEventLevel.Information, DefaultLogConfigurationUseCase.LogTemplate);
        AddSeq();
    }

    private static readonly FileSinkOptions Options = new()
    {
        Formatter = new JsonFormatter(),
        Path = Path.Combine(Path.GetTempPath(), "TestLogs", "logs.txt"),
        RestrictedToMinimumLevel = LogEventLevel.Information,
        RollingInterval = RollingInterval.Day,
        RetainedFileCountLimit = 3,
        RollOnFileSizeLimit = true,
        FileSizeLimitBytes = (long)(300 * Math.Pow(1024, 2)),
        Shared = true,
        Encoding = Encoding.UTF8,
        RetainedFileTimeLimit = TimeSpan.FromDays(1)
    };

    public void AddFileSink(Func<FileSinkOptions, FileSinkOptions>? options = null)
    {
        var opts = options?.Invoke(Options) ?? Options;

        Configuration.WriteTo.File(opts);
    }

    public void AddEnrichments()
    {
        Base.AddEnrichments();
        AppDataRecord
            .Parse(AppDomain.CurrentDomain.BaseDirectory)
            .Enrich(Configuration);
    }

    public void AddLogFilters()
    {
        var filterHandler = new FilterHandler();
        filterHandler.AddStringFilter(
                [
                    x => x.Contains("CREATE"),
                    x => x.Contains("PRAGMA"),
                    x => x.Contains("query uses the 'First'/'FirstOrDefault' operator")
                ]
            )
            .AddLogFilter(
                [
                    x => x.HasSourceContext("Microsoft.EntityFrameworkCore.Database.Command"),
                ]
            );
        Configuration.Filter.ByExcluding(filterHandler.FilterByExcluding);
    }

    private void AddSeq()
    {
        var url = Environment.GetEnvironmentVariable("SEQ_URL");


        var key = Environment.GetEnvironmentVariable("SEQ_API_KEY");


        var log = Log.ForContext<LogConfigurationTestUseCase>();
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
        {
            log.Warning("Seq URL or API key not found. Seq sink not added.");
            return;
        }

        Configuration.WriteTo.Seq(url, LogEventLevel.Information, apiKey: key);
        log.Information("Seq sink added.");
    }
}

file class FilterHandler
{
    public List<Func<string, bool>> StringExclusionFilters { get; } = [];
    public List<Func<LogEvent, bool>> LogExclusionFilters { get; } = [];

    public FilterHandler AddStringFilter(Func<string, bool> filter)
    {
        StringExclusionFilters.Add(filter);
        return this;
    }

    public FilterHandler AddStringFilter(IEnumerable<Func<string, bool>> filters)
    {
        StringExclusionFilters.AddRange(filters);
        return this;
    }

    public FilterHandler AddLogFilter(Func<LogEvent, bool> filter)
    {
        LogExclusionFilters.Add(filter);
        return this;
    }

    public FilterHandler AddLogFilter(IEnumerable<Func<LogEvent, bool>> filters)
    {
        LogExclusionFilters.AddRange(filters);
        return this;
    }

    public bool FilterByExcluding(LogEvent logEvent)
    {
        var renderedMessage = logEvent.RenderMessage();
        return LogExclusionFilters
            .Select(x => x(logEvent))
            .Concat(StringExclusionFilters.Select(x => x(renderedMessage)))
            .Any(x => x);
    }
}

public static class LogEventExtensions
{
    private const string SourceContext = "SourceContext";

    public static bool HasSourceContext(this LogEvent logEvent, string sourceContext)
    {
        return logEvent.GetSourceContextString() is { } s && s == sourceContext;
    }

    public static bool SourceContextContains(this LogEvent logEvent, string sourceContext)
    {
        return logEvent.GetSourceContextString() is { } s && s.Contains(sourceContext);
    }

    public static string? GetSourceContextString(this LogEvent logEvent)
    {
        return logEvent.Properties.TryGetValue(SourceContext, out var value) &&
               value is ScalarValue { Value: string sourceContext }
            ? sourceContext
            : null;
    }
}
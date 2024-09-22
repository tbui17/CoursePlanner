using System.Text;
using Lib.Config;
using Lib.Utils;
using Serilog;
using Serilog.Context;
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
        if (Environment.GetEnvironmentVariable("CI") is not null)
        {
            Configuration.WriteTo.Console(new JsonFormatter(), LogEventLevel.Error);
            Configuration.WriteTo.File(Options with { RestrictedToMinimumLevel = LogEventLevel.Warning });
            return;
        }




        Configuration
            .WriteTo.Console(LogEventLevel.Information, DefaultLogConfigurationUseCase.LogTemplate);

        Configuration.WriteTo.File(Options);
        AddSeq();
    }

    private static readonly FileSinkOptions Options = new()
    {
        Formatter = new JsonFormatter(),
        Path = Path.Combine(Path.GetTempPath(), "TestReports", "logs.txt"),
        RestrictedToMinimumLevel = LogEventLevel.Information,
        RollingInterval = RollingInterval.Day,
        RetainedFileCountLimit = 3,
        RollOnFileSizeLimit = true,
        FileSizeLimitBytes = (long)(300 * Math.Pow(1024, 2)),
        Shared = true,
        Encoding = Encoding.UTF8,
        RetainedFileTimeLimit = TimeSpan.FromDays(1)
    };

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

        filterHandler
            .AddStringFilter(
                [
                    x => x.Contains("CREATE"),
                    x => x.Contains("PRAGMA"),
                    x => x.Contains("query uses the 'First'/'FirstOrDefault' operator")
                ]
            )
            .AddLogFilter(
                [
                    x => x.HasSourceContext(nameof(Microsoft.EntityFrameworkCore.ChangeTracking).ToNamespaceString()),
                    x => x.HasSourceContext(nameof(Microsoft.EntityFrameworkCore.ChangeTracking).ToNamespaceString()),
                    x => x.HasSourceContext(
                        $"{nameof(Microsoft.EntityFrameworkCore).ToNamespaceString()}.Database.Command"
                    )
                ]
            )
            .AddInclusionFilter(
                [
                    x => x.Level is LogEventLevel.Information && x.SourceContextContains("Base")
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
            log.Information("Seq URL or API key not found. Seq sink not added.");
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
    public List<Func<LogEvent, bool>> InclusionFilters { get; } = [];

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

    public FilterHandler AddInclusionFilter(IEnumerable<Func<LogEvent, bool>> filters)
    {
        InclusionFilters.AddRange(filters);
        return this;
    }

    public bool FilterByExcluding(LogEvent logEvent)
    {
        if (InclusionFilters.Any(x => x(logEvent)))
        {
            return false;
        }

        if (LogExclusionFilters.Any(x => x(logEvent)))
        {
            return true;
        }

        var renderedMessage = logEvent.RenderMessage();
        return StringExclusionFilters.Any(x => x(renderedMessage));
    }
}

public static class LogEventExtensions
{
    private const string SourceContext = "SourceContext";

    public static bool HasSourceContext(this LogEvent logEvent, string sourceContext)
    {
        return logEvent.GetSourceContextString() is { } s && s == sourceContext;
    }

    public static bool HasSourceContextClass(this LogEvent logEvent, string sourceContext)
    {
        return logEvent.GetSourceContextClass() == sourceContext;
    }

    public static string? GetSourceContextClass(this LogEvent logEvent)
    {
        return logEvent
            .GetSourceContextString()
            ?.Split(".")
            .LastOrDefault();
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
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

    private static readonly Func<string, bool> InclusionFilters = new Func<string, bool>[]
        {
            x => x.Contains("CREATE"),
            x => x.Contains("PRAGMA"),
            x => x.Contains("Microsoft.EntityFrameworkCore.Database")
        }
        .ToAnyPredicate();

    public void AddLogFilters()
    {
        Configuration.Filter.ByExcluding(x => x.RenderMessage().Thru(InclusionFilters));
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


using System.Text;
using Lib.Config;
using Lib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace BaseTestSetup;

internal sealed class LogConfigurationTestUseCase :  ILogConfigurationBuilder<LogConfigurationTestUseCase>
{

    public LoggerConfiguration Configuration { get; set; } = new();

    private DefaultLogConfigurationUseCase Base { get; set; }

    public LogConfigurationTestUseCase()
    {
        Base = new DefaultLogConfigurationUseCase {Configuration = Configuration};
    }


    public LogConfigurationTestUseCase SetMinimumLogLevel(Action<LoggerMinimumLevelConfiguration> setter)
    {
        Base.SetMinimumLogLevel(setter);
        return this;
    }

    public LogConfigurationTestUseCase AddDefaultSinks()
    {
        Base.AddDefaultSinks();
        Configuration
            .WriteTo.Debug(LogEventLevel.Debug, DefaultLogConfigurationUseCase.LogTemplate);
        return this;
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

    public LogConfigurationTestUseCase AddFileSink(Func<FileSinkOptions, FileSinkOptions>? options = null)
    {


        var opts = options?.Invoke(Options) ?? Options;

        Configuration.WriteTo.File(opts);

        return this;
    }

    public LogConfigurationTestUseCase AddDefaultEnrichments()
    {
        Base.AddDefaultEnrichments();
        AppDataRecord.Parse(AppDomain.CurrentDomain.BaseDirectory).Enrich(Configuration);
        return this;
    }

    public LogConfigurationTestUseCase AddLogFilters()
    {
        var shouldExcludeMessage = new Func<string, bool>[]
            {
                x => x.Contains("CREATE"),
                x => x.Contains("PRAGMA"),
            }
            .ToAnyPredicate();
        Configuration.Filter.ByExcluding(x => x.RenderMessage().Thru(shouldExcludeMessage));
        return this;
    }

    public LogConfigurationTestUseCase AddSeq(string? seqUrl = default, string? apiKey = default)
    {
        string? url = null;
        string? key = null;
        if (string.IsNullOrWhiteSpace(seqUrl))
        {
            url = Environment.GetEnvironmentVariable("SEQ_URL");
        }
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            key = Environment.GetEnvironmentVariable("SEQ_API_KEY");
        }

        if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(key))
        {
            Configuration.WriteTo.Seq(url, LogEventLevel.Information, apiKey: key);
        }

        return this;
    }

    public IServiceCollection Finalize(IServiceCollection services, bool useGlobalLogger = true)
    {
        return Base.Finalize(services, useGlobalLogger);
    }


}
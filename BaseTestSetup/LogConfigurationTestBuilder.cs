using System.Text;
using Lib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace BaseTestSetup;

internal class LogConfigurationTestBuilder :  ILogConfigurationBuilder<LogConfigurationTestBuilder>
{

    public LoggerConfiguration Configuration { get; set; } = new();

    private DefaultLogConfigurationBuilder Base { get; set; }

    public LogConfigurationTestBuilder()
    {
        Base = new DefaultLogConfigurationBuilder {Configuration = Configuration};
    }


    public LogConfigurationTestBuilder SetMinimumLogLevel(Action<LoggerMinimumLevelConfiguration> setter)
    {
        Base.SetMinimumLogLevel(setter);
        return this;
    }

    public LogConfigurationTestBuilder AddDefaultSinks()
    {
        Base.AddDefaultSinks();
        Configuration
            .WriteTo.Debug(LogEventLevel.Debug, DefaultLogConfigurationBuilder.LogTemplate);
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

    public LogConfigurationTestBuilder AddFileSink(Func<FileSinkOptions, FileSinkOptions>? options = null)
    {


        var opts = options?.Invoke(Options) ?? Options;

        Configuration.WriteTo.File(opts);

        return this;
    }

    public LogConfigurationTestBuilder AddDefaultEnrichments()
    {
        Base.AddDefaultEnrichments();
        AppDataRecord.Parse(AppDomain.CurrentDomain.BaseDirectory).Enrich(Configuration);
        return this;
    }

    public LogConfigurationTestBuilder AddLogFilters()
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

    public IServiceCollection Finalize(IServiceCollection services, bool useGlobalLogger = true)
    {
        return Base.Finalize(services, useGlobalLogger);
    }

    public LogConfigurationTestBuilder AddSeq()
    {
        var url = Environment.GetEnvironmentVariable("SEQ_URL");
        var apiKey = Environment.GetEnvironmentVariable("SEQ_API_KEY");
        if (url is not null && apiKey is not null)
        {
            Configuration.WriteTo.Seq(url, LogEventLevel.Information, apiKey: apiKey);
        }

        return this;
    }
}
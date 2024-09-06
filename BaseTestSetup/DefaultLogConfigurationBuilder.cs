using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace BaseTestSetup;

public class DefaultLogConfigurationBuilder : ILogConfigurationBuilder<DefaultLogConfigurationBuilder>
{
    public LoggerConfiguration Configuration { get; set; } = new();

    public const string LogTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";


    public DefaultLogConfigurationBuilder SetMinimumLogLevel(Action<LoggerMinimumLevelConfiguration> setter)
    {
        setter(Configuration.MinimumLevel);
        return this;
    }

    public DefaultLogConfigurationBuilder AddDefaultSinks()
    {
        Configuration.WriteTo.Console(LogEventLevel.Information, LogTemplate);
        return this;
    }

    public DefaultLogConfigurationBuilder AddDefaultEnrichments()
    {
        Configuration
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("FriendlyApplicationName", AppDomain.CurrentDomain.FriendlyName)
            .Enrich.With(new StackTraceEnricher());

        return this;
    }

    private static readonly FileSinkOptions Options = new()
    {
        Path = Path.Combine("logs", "logs.json"),
        RestrictedToMinimumLevel = LogEventLevel.Information,
        RollingInterval = RollingInterval.Infinite,
        RetainedFileCountLimit = 3,
        RollOnFileSizeLimit = true,
        Shared = true,
        Buffered = true
    };

    public DefaultLogConfigurationBuilder AddFileSink(Func<FileSinkOptions, FileSinkOptions>? options = null)
    {
        var opts = options?.Invoke(Options) ?? Options;
        Configuration.WriteTo.File(opts);
        return this;
    }


    public DefaultLogConfigurationBuilder AddLogFilters()
    {
        return this;
    }


    public IServiceCollection Finalize(IServiceCollection services, bool useGlobalLogger = true)
    {
        Logger? logger = null;
        if (useGlobalLogger)
        {
            Log.Logger = Configuration.CreateLogger();
        }
        else
        {
            logger = Configuration.CreateLogger();
        }

        return services
            .AddSerilog(logger, dispose: true)
            .AddLogging();
    }
}
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace Lib.Config;

public interface IDefaultLogConfigurationBuilder : ILogConfigurationBuilder<IDefaultLogConfigurationBuilder>;

public class DefaultLogConfigurationUseCase : IDefaultLogConfigurationBuilder
{
    public LoggerConfiguration Configuration { get; set; } = new();

    public const string LogTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";


    public IDefaultLogConfigurationBuilder SetMinimumLogLevel(Action<LoggerMinimumLevelConfiguration> setter)
    {
        setter(Configuration.MinimumLevel);
        return this;
    }

    public IDefaultLogConfigurationBuilder AddDefaultSinks()
    {
        Configuration.WriteTo.Console(LogEventLevel.Information, LogTemplate);
        return this;
    }

    public IDefaultLogConfigurationBuilder AddDefaultEnrichments()
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

    public IDefaultLogConfigurationBuilder AddFileSink(Func<FileSinkOptions, FileSinkOptions>? options = null)
    {
        var opts = options?.Invoke(Options) ?? Options;
        Configuration.WriteTo.File(opts);
        return this;
    }


    public IDefaultLogConfigurationBuilder AddLogFilters()
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
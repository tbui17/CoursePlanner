using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace Lib.Config;

public interface ILogConfigurationBuilder<out T> where T : ILogConfigurationBuilder<T>
{
    LoggerConfiguration Configuration { get; set; }
    T SetMinimumLogLevel(Action<LoggerMinimumLevelConfiguration> setter);
    T AddDefaultSinks();
    T AddDefaultEnrichments();
    T AddFileSink(Func<FileSinkOptions, FileSinkOptions>? options = null);
    T AddLogFilters();
    IServiceCollection Finalize(IServiceCollection services, bool useGlobalLogger = true);
}
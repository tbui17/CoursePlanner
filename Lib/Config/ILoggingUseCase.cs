using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Lib.Config;

public interface ILoggingUseCase
{
    LoggerConfiguration Configuration { get; set; }
    void SetMinimumLogLevel();
    void AddSinks();
    void AddEnrichments();
    void AddLogFilters();
}

public static class LogConfigurationBuilderExtensions
{
    public static IServiceCollection AddLoggingUseCase(this IServiceCollection services, ILoggingUseCase useCase)
    {
        useCase.SetMinimumLogLevel();
        useCase.AddSinks();
        useCase.AddEnrichments();
        useCase.AddLogFilters();

        Log.Logger = useCase.Configuration.CreateLogger();

        return services
            .AddSerilog(dispose:true)
            .AddLogging();
    }
}
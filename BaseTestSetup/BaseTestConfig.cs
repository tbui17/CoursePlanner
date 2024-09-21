using FluentAssertions.Extensions;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using Polly.Retry;
using Polly.Telemetry;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;

namespace BaseTestSetup;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ILocalDbCtxFactory = Microsoft.EntityFrameworkCore.IDbContextFactory<LocalDbCtx>;

public static class BaseTestConfig
{
    public static IServiceCollection AddLogger(this IServiceCollection services)
    {
        return services.AddLoggingUseCase(new LogConfigurationTestUseCase());
    }

    public static IServiceCollection AddTestDatabase(this IServiceCollection services, string? fileName = null)
    {
        var name = fileName ?? Guid.NewGuid().ToString();
        return services.AddDbContext<LocalDbCtx>(x => x
                .UseSqlite($"DataSource={name}")
            )
            .AddDbContextFactory<LocalDbCtx>();
    }

    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        return services;
    }

    public static async Task GlobalTearDown()
    {
        await Log.CloseAndFlushAsync();
    }
}

public abstract class BaseConfigTest : IBaseTest
{
    public IServiceProvider Provider { get; set; } = null!;

    protected abstract IDisposable TestContextProvider();

    protected IDisposable CreateLogContext(Dictionary<string, object?> properties)
    {
        var props = properties.Select(x => new PropertyEnricher(x.Key, x.Value, destructureObjects: true))
            .Cast<ILogEventEnricher>()
            .ToArray();

        return LogContext.Push(props);
    }


    public virtual Task Setup()
    {
        Provider = CreateProvider();
        return Task.CompletedTask;
    }

    public virtual ServiceCollection CreateServicesContainer()
    {
        var services = new ServiceCollection();

        services
            .AddResiliencePipeline("Retry",
                builder => builder.AddRetry(new RetryStrategyOptions
                        {
                            OnRetry = onRetryArguments =>
                            {
                                Log.Error(onRetryArguments.Outcome.Exception,
                                    "Error occurred. Retrying. {@OnRetryArguments}",onRetryArguments
                                );
                                return ValueTask.CompletedTask;
                            },
                            MaxRetryAttempts = 3,
                            Delay = 1.Seconds(),
                            BackoffType = DelayBackoffType.Linear,
                            ShouldHandle = x =>
                            {
                                var shouldHandle = x.Outcome.Exception switch
                                {
                                    IOException e when e.Message.Contains(
                                        "The process cannot access the file",
                                        StringComparison.OrdinalIgnoreCase
                                    ) => true,
                                    _ => false
                                };


                                return ValueTask.FromResult(shouldHandle);
                            }
                        }
                    )
                    .AddTimeout(10.Seconds())
            )
            .AddLogger()
            .AddInjectables()
            .AddBackendServices()
            .AddTestDatabase()
            .AddLogging()
            .AddTestServices();
        return services;
    }


    public virtual IServiceProvider CreateProvider()
    {
        return CreateServicesContainer().BuildServiceProvider();
    }

    public virtual Task TearDown()
    {
        return Task.CompletedTask;
    }

    protected ResiliencePipeline ResolveResiliencePipeline(string name) =>
        Provider.GetRequiredService<ResiliencePipelineProvider<string>>().GetPipeline(name);

    public virtual T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();

    public virtual async Task<LocalDbCtx> GetDb() => await Resolve<ILocalDbCtxFactory>().CreateDbContextAsync();
}
using Lib.Attributes;
using Lib.Config;
using Lib.Models;

namespace BaseTestSetup;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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


    public virtual Task Setup()
    {
        Provider = CreateProvider();
        return Task.CompletedTask;
    }

    public virtual ServiceCollection CreateServicesContainer()
    {
        var services = new ServiceCollection();

        services
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


    public virtual async Task TearDown()
    {
        await Task.CompletedTask;
    }

    public virtual T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();

    public virtual async Task<LocalDbCtx> GetDb() => await Resolve<IDbContextFactory<LocalDbCtx>>().CreateDbContextAsync();
}

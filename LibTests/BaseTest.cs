using BaseTestSetup;
using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace LibTests;

public abstract class BaseTest : IBaseTest
{
    public IServiceProvider Provider { get; set; }

    [SetUp]
    public virtual Task Setup()
    {
        Provider = CreateProvider();
        return Task.CompletedTask;
    }

    private IServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddBackendServices()
            .AddLogger(x => Log.Logger = x)
            .AddTestDatabase()
            .AddTransient<NotificationSetupUtil>();

        return services.BuildServiceProvider();
    }

    [TearDown]
    public virtual async Task TearDown()
    {
        await Task.CompletedTask;
    }

    public T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();

    public async Task<LocalDbCtx> GetDb() => await Resolve<IDbContextFactory<LocalDbCtx>>().CreateDbContextAsync();
}
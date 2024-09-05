using BaseTestSetup;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


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

        services
            .AddLogger()
            .AddInjectables()
            .AddBackendServices()
            .AddTestDatabase()
            .AddLogging();

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
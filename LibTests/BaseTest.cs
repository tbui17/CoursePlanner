using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace LibTests;

public abstract class BaseTest
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
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Debug()
            .Enrich.FromLogContext()
            .CreateLogger();

        var guid = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        Configs
            .AddBackendServices(services)
            .AddSerilog()
            .AddDbContext<LocalDbCtx>(x => x
                .UseSqlite($"DataSource={guid}")
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
            )
            .AddDbContextFactory<LocalDbCtx>()
            .AddTransient<NotificationSetupUtil>();

        services.AddLogging();
        return services.BuildServiceProvider();
    }

    [TearDown]
    protected virtual async Task TearDown()
    {
        await Task.CompletedTask;
    }

    public T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();

    public async Task<LocalDbCtx> GetDb() => await Resolve<IDbContextFactory<LocalDbCtx>>().CreateDbContextAsync();
}
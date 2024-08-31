using Lib;
using Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.Services;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace ViewModelTests.TestSetup;

public abstract class BaseTest
{
    [SetUp]
    public virtual Task Setup()
    {
        Connection = new SqliteConnectionStringBuilder
        {
            DataSource = $"{FileName}.db"
        };

        Provider = CreateProvider();
        return Task.CompletedTask;
    }

    protected string FileName { get; private init; } = Guid.NewGuid().ToString();

    protected IServiceProvider Provider { get; private set; }

    protected SqliteConnectionStringBuilder Connection { get; private set; }

    private IServiceProvider CreateProvider()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Debug()
            .Enrich.FromLogContext()
            .CreateLogger();

        var mockNavigation = new Mock<INavigationService>();
        var mockAppService = new Mock<IAppService>();
        var services = new ServiceCollection();
        Configs
            .AddBackendServices(services)
            .AddServices()
            .AddSerilog()
            .AddAssemblyNames([nameof(ViewModelTests), nameof(ViewModels)])
            .AddDbContext<LocalDbCtx>(x => x
                .UseSqlite(Connection.ToString())
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
            )
            .AddDbContextFactory<LocalDbCtx>()
            .AddSingleton(mockNavigation.Object)
            .AddSingleton(mockAppService.Object)
            .AddTransient<ISessionService, SessionService>()
            .AddTransient<AppShellViewModel>();

        services.AddLogging();

        return services.BuildServiceProvider();
    }

    protected T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();


    protected LocalDbCtx GetDb() =>
        Provider
            .GetRequiredService<ILocalDbCtxFactory>()
            .CreateDbContext();

    [TearDown]
    protected virtual async Task TearDown()
    {
        await Task.CompletedTask;
    }
}
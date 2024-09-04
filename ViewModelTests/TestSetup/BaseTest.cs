using BaseTestSetup;
using Lib;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Lib.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Serilog;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.Services;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace ViewModelTests.TestSetup;

public abstract class BaseTest : IBaseTest
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

    public IServiceProvider Provider { get; set; }

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
        var assemblyService = new AssemblyService(AppDomain.CurrentDomain);
        var backendConfig = new BackendConfig(assemblyService, services);
        backendConfig.AddServices();
        var vmConfig = new ViewModelConfig(assemblyService, services);
        vmConfig.AddServices();
        services.AddInjectables(AppDomain.CurrentDomain);
        services
            .AddSerilog()
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

    public T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();


    public Task<LocalDbCtx> GetDb() =>
        Provider
            .GetRequiredService<ILocalDbCtxFactory>()
            .CreateDbContextAsync();

    [TearDown]
    public virtual async Task TearDown()
    {
        await Task.CompletedTask;
    }
}
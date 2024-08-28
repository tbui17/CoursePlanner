using Lib;
using Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ViewModels.Config;
using ViewModels.PageViewModels;
using ViewModels.Services;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;

[assembly: NonParallelizable]
// [assembly:Parallelizable(ParallelScope.All)]
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

    protected string FileName { get; private init; } = "database";

    protected IServiceProvider Provider { get; private set; }

    protected SqliteConnectionStringBuilder Connection { get; private set; }

    private IServiceProvider CreateProvider()
    {
        var mockNavigation = new Mock<INavigationService>();
        var mockAppService = new Mock<IAppService>();
        var services = new ServiceCollection();
        Configs
            .ConfigBackendServices(services)
            .ConfigServices()
            .ConfigAssemblyNames([nameof(ViewModelTests), nameof(ViewModels)])
            .AddDbContext<LocalDbCtx>(x => x
                .UseSqlite(Connection.ToString())
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine)
            )
            .AddDbContextFactory<LocalDbCtx>()
            .AddSingleton(mockNavigation.Object)
            .AddSingleton(mockAppService.Object)
            .AddTransient<ISessionService, SessionService>()
            .AddTransient<AppShellViewModel>();

        services.AddLogging(b => b.AddConsole());

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

    }

}
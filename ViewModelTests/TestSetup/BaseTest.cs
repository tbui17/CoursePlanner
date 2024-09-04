using BaseTestSetup;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Lib.Utils;
using Microsoft.Data.Sqlite;
using Serilog;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.Services;


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


        var services = new ServiceCollection();
        var assemblyService = new AssemblyService(AppDomain.CurrentDomain);
        var backendConfig = new BackendConfig(assemblyService, services);
        backendConfig.AddServices();
        var vmConfig = new ViewModelConfig(assemblyService, services);
        vmConfig.AddServices();

        services
            .AddInjectables(AppDomain.CurrentDomain)
            .AddLogger(x => Log.Logger = x.CreateLogger())
            .AddTestDatabase()
            .AddTransient<ISessionService, SessionService>()
            .AddTransient<AppShellViewModel>();

        return services.BuildServiceProvider();
    }

    public T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();


    public async Task<LocalDbCtx> GetDb() =>
        await Provider
            .GetRequiredService<ILocalDbCtxFactory>()
            .CreateDbContextAsync();

    [TearDown]
    public virtual async Task TearDown()
    {
        await Task.CompletedTask;
    }
}
using BaseTestSetup;
using Lib.Attributes;
using Lib.Models;
using Microsoft.Data.Sqlite;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

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

    public static IServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        var vmConfig = new ViewModelConfig(services);


        services
            .AddInjectables()
            .AddLogger()
            .AddTestDatabase()
            .AddTransient<ISessionService, SessionService>()
            .AddTransient<AppShellViewModel>()
            .AddTransient<NotificationDataPaginationTestFixtureDataFactory>(x =>
                new NotificationDataPaginationTestFixtureDataFactory(CreateFixture(), x)
            )
            .AddTransient<NotificationDataPaginationTestFixture>(x =>
                x.GetRequiredService<NotificationDataPaginationTestFixtureDataFactory>().CreateFixture()
            );
        vmConfig.AddServices();

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
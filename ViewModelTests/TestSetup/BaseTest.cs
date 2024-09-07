using AutoFixture;
using AutoFixture.AutoMoq;
using BaseTestSetup;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Lib.Utils;
using Microsoft.Data.Sqlite;
using Moq;
using Serilog;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.Services;

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
        var vmConfig = new ViewModelConfig(assemblyService, services);


        services
            .AddInjectables()
            .AddLogger()
            .AddTestDatabase()
            .AddTransient<ISessionService, SessionService>()
            .AddTransient<AppShellViewModel>();
        vmConfig.AddServices();

        return services.BuildServiceProvider();
    }

    public T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();

    protected IFixture CreateFixture()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    protected Mock<T> CreateMock<T>() where T : class => CreateFixture().Create<Mock<T>>();


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
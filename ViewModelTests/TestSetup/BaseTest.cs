using BaseTestSetup;
using Lib.Attributes;
using Lib.Models;
using Microsoft.Data.Sqlite;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

namespace ViewModelTests.TestSetup;

public abstract class BaseTest : BaseConfigTest, IBaseTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
    }


    public override IServiceProvider CreateProvider()
    {

        var services = base.CreateServicesContainer();
        var vmConfig = new ViewModelConfig(services);


        services
            .AddTransient<ISessionService, SessionService>()
            .AddTransient<AppShellViewModel>()
            .AddTransient<NotificationDataPaginationTestFixtureDataFactory>(x =>
                new NotificationDataPaginationTestFixtureDataFactory(CreateFixture(), x)
            )
            .AddTransient<NotificationDataPaginationTestFixture>(x => x.GetRequiredService<NotificationDataPaginationTestFixtureDataFactory>().CreateFixture());
        vmConfig.AddServices();

        return services.BuildServiceProvider();
    }

    [TearDown]
    public override async Task TearDown()
    {
        await base.TearDown();
    }
}
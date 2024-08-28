using AutoFixture;
using AutoFixture.AutoMoq;
using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ViewModels.Config;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests.TestSetup;

public static class Globals
{
    // private static IServiceProvider? _provider;
    // public static IServiceProvider Provider => _provider ??= CreateProvider();
    //
    //
    // private static IServiceProvider CreateProvider()
    // {
    //     var mockNavigation = new Mock<INavigationService>();
    //     var mockAppService = new Mock<IAppService>();
    //     var services = new ServiceCollection();
    //     Configs
    //         .ConfigBackendServices(services)
    //         .ConfigServices()
    //         .ConfigAssemblyNames([nameof(ViewModelTests), nameof(ViewModels)])
    //         .AddDbContext<LocalDbCtx>(x => x
    //             .UseSqlite("DataSource=database.db")
    //             .EnableDetailedErrors()
    //             .EnableSensitiveDataLogging()
    //             .LogTo(Console.WriteLine)
    //         )
    //         .AddDbContextFactory<LocalDbCtx>()
    //         .AddSingleton(mockNavigation.Object)
    //         .AddSingleton(mockAppService.Object)
    //         .AddTransient<ISessionService, SessionService>()
    //         .AddTransient<AppShellViewModel>();
    //
    //     services.AddLogging(b => b.AddConsole());
    //
    //     return services.BuildServiceProvider();
    // }
    //
    // public static T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();
    //
    //
    // public static LocalDbCtx GetDb() =>
    //     Provider
    //         .GetRequiredService<ILocalDbCtxFactory>()
    //         .CreateDbContext();
    //
    public static IFixture CreateFixture()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    public static T CreateMock<T>() where T : class => CreateFixture().Create<T>();
}
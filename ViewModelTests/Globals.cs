using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests;

public static class Globals
{
    private static IServiceProvider? _provider;
    public static IServiceProvider Provider => _provider ??= CreateProvider();


    private static IServiceProvider CreateProvider()
    {
        var mockNavigation = new Mock<INavigationService>();
        var mockAppService = new Mock<IAppService>();
        var services = new ServiceCollection();
        Configs
           .ConfigBackendServices(services)
           .AddDbContext<LocalDbCtx>(x => x
               .UseSqlite("DataSource=database.db")
               .EnableDetailedErrors()
               .EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine)
            )
           .AddDbContextFactory<LocalDbCtx>()
           .AddSingleton(mockNavigation.Object)
           .AddSingleton(mockAppService.Object)
           .AddSingleton<EditTermViewModel>()
            ;
        return services.BuildServiceProvider();
    }

    public static T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();

    public static Func<LocalDbCtx> GetDbFactory() => () => Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>().CreateDbContext();
}
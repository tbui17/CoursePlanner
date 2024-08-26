using Lib;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ViewModels.Events;
using ViewModels.Interfaces;
using ViewModels.PageViewModels;
using ViewModels.Services;
using ViewModels.Utils;

namespace ViewModelTests.TestSetup;

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
            .AddSingleton<NavigationSubject>()
            .AddTransient<DetailedCourseViewModel>()
            .AddTransient<DetailedTermViewModel>()
            .AddTransient<EditAssessmentViewModel>()
            .AddTransient<EditCourseViewModel>()
            .AddTransient<EditTermViewModel>()
            .AddTransient<MainViewModel>()
            .AddTransient<InstructorFormViewModelFactory>()
            .AddSingleton<ILocalNotificationService, LocalNotificationService>()
            .AddTransient<ISessionService, SessionService>()
            .AddTransientRefreshable<TermViewModel>()
            .AddTransient<ReflectionUtil>(_ => new ReflectionUtil{AssemblyNames = [nameof(ViewModelTests),nameof(ViewModels)]})
            .AddTransient<AppShellViewModel>();

        services.AddLogging(b => b.AddConsole());

        return services.BuildServiceProvider();
    }

    private static IServiceCollection AddTransientRefreshable<T>(this IServiceCollection services)
        where T : class, IRefresh =>
        services.AddTransient<T>(x =>
        {
            var subj = x.GetRequiredService<NavigationSubject>();
            var instance = x.CreateInstance<T>();
            subj.Subscribe(instance);
            return instance;
        });

    public static T Resolve<T>() where T : notnull => Provider.GetRequiredService<T>();


    public static LocalDbCtx GetDb() =>
        Provider
            .GetRequiredService<ILocalDbCtxFactory>()
            .CreateDbContext();
}

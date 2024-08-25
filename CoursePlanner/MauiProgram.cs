using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
using CoursePlanner.Views;
using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace CoursePlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .UseLocalNotification()
            .ConfigureFonts
            (
                fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }
            );


        Configs
            .ConfigBackendServices(builder.Services)
            .AddDbContext<LocalDbCtx>(b =>
                {
                    b
                        .UseSqlite($"DataSource={FileSystem.Current.AppDataDirectory}/database.db")
                        .LogTo(Console.WriteLine);
#if DEBUG
                    b.EnableSensitiveDataLogging();
                    b.EnableDetailedErrors();
#endif
                },
                ServiceLifetime.Transient
            )
            .AddDbContextFactory<LocalDbCtx>(lifetime: ServiceLifetime.Transient)
            .AddSingleton<AppService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IAppService, AppService>()
            .AddSingleton<ILocalNotificationService, LocalNotificationService>(x =>
            {
                var instance = x.CreateInstance<LocalNotificationService>();
                instance.StartListening();
                return instance;
            })
            .AddSingleton<AppShell>()
            .AddSingleton<AppShellViewModel>()
            .AddSingleton<ISessionService, SessionService>()
            .AddTransient<MainViewModel>()
            .AddTransient<MainPage>()
            .AddTransient<DetailedTermViewModel>()
            .AddTransient<DetailedTermPage>()
            .AddTransient<EditTermPage>()
            .AddTransient<EditTermViewModel>()
            .AddTransient<DetailedCoursePage>()
            .AddTransient<DetailedCourseViewModel>()
            .AddTransient<EditCoursePage>()
            .AddTransient<EditCourseViewModel>()
            .AddTransient<InstructorFormViewModelFactory>()
            // .AddTransient<InstructorFormViewModel>() // see NavigationService
            // .AddTransient<InstructorFormPage>()
            .AddTransient<EditNotePage>()
            .AddTransient<EditNoteViewModel>()
            .AddTransient<EditAssessmentPage>()
            .AddTransient<EditAssessmentViewModel>()
            .AddTransient<LoginViewModel>()
            .AddTransient<TermViewModel>()
            .AddTransient<TermListView>()
            .AddTransient<DevPage>()
            .AddTransient<DbSetup>()
            .AddTransient<LoginView>()
            .AddTransient<NotificationDataPage>()
            .AddTransient<NotificationDataViewModel>();

        builder.Logging.AddConsole();

#if DEBUG
        builder
            .Logging
            .AddDebug();
#endif


        var app = builder.Build();

        app
            .Services
            .GetRequiredService<DbSetup>()
            .SetupDb();

        return app;
    }
}

file static class ContainerExtensions
{
    public static T CreateInstance<T>(this IServiceProvider provider)
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }

    public static IServiceCollection AddTransient2<T>(this IServiceCollection services, Func<AddTransientCtx<T>, T> fn)
        where T : class
    {
        services.AddTransient(provider =>
        {
            var instance = fn(new AddTransientCtx<T>(provider.CreateInstance<T>(), provider));
            return instance;
        });
        return services;
    }

    public record AddTransientCtx<T>(T Instance, IServiceProvider Provider)
    {
        public static implicit operator T(AddTransientCtx<T> ctx) => ctx.Instance;
    };
}
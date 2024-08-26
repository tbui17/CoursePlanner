using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
using CoursePlanner.Views;
using Lib;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ViewModels.PageViewModels;
using ViewModels.Services;
using ViewModels.Utils;

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
            .AddTransient<NotificationDataViewModel>()
            .AddTransient<ReflectionUtil>(_ => new ReflectionUtil { AssemblyNames = [nameof(CoursePlanner)] });

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
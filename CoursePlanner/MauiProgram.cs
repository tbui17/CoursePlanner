using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
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
           .AddDbContext<LocalDbCtx>(x => x
                   .UseSqlite($"DataSource={FileSystem.Current.AppDataDirectory}/database.db")
                   .EnableSensitiveDataLogging()
                   .EnableDetailedErrors()
                   .LogTo(Console.WriteLine),
                ServiceLifetime.Transient
            )
           .AddDbContextFactory<LocalDbCtx>(lifetime: ServiceLifetime.Transient)
           .AddSingleton<AppService>()
           .AddSingleton<INavigationService, AppService>()
           .AddSingleton<IAppService, AppService>()
           .AddSingleton<ILocalNotificationService, LocalNotificationService>()
           .AddTransient<MainPage, MainViewModel>()
           .AddTransient<DetailedTermPage, DetailedTermViewModel>()
           .AddTransient<EditTermPage, EditTermViewModel>()
           .AddTransient<DetailedCoursePage, DetailedCourseViewModel>()
           .AddTransient<EditCoursePage, EditCourseViewModel>()
           .AddTransient<InstructorFormPage, InstructorFormViewModel>()
           .AddTransient<EditNotePage, EditNoteViewModel>()
           .AddTransient<EditAssessmentPage, EditAssessmentViewModel>()
           .AddTransient<DevPage>()
           .AddTransient<DbSetup>();

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
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.Services;
using CoursePlanner.ViewModels;
using Lib;
using Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
#if ANDROID
using Android.Util;
#endif

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
           .AddSingleton<AppService>()
           .AddTransient<MainPage, MainViewModel>()
           .AddTransient<DetailedTermPage, DetailedTermViewModel>()
           .AddTransient<EditTermPage, EditTermViewModel>()
           .AddTransient<DetailedCoursePage, DetailedCourseViewModel>()
           .AddTransient<EditCoursePage, EditCourseViewModel>()
           .AddTransient<InstructorFormPage, InstructorFormViewModel>()
           .AddTransient<EditNotePage, EditNoteViewModel>()
           .AddTransient<EditAssessmentPage, EditAssessmentViewModel>()
           .AddTransient<DevPage>();

        builder.Logging.AddConsole();

#if DEBUG
        builder
           .Logging
           .AddDebug();
#endif


        var app = builder.Build();

        SetupDb();

        return app;


        void SetupDb()
        {

            using var db = new LocalDbCtx{ ApplicationDirectoryPath = FileSystem.Current.AppDataDirectory };
            var logger = app.Services.GetRequiredService<ILogger<LocalDbCtx>>();
            logger.LogInformation("Attempting to migrate database.");
            try
            {
                db.Database.Migrate();
                logger.LogInformation("Database migrated.");
            }
            catch (SqliteException e) when (e.Message.Contains("already exists"))
            {
                logger.LogInformation($"A database entity already exists. Attempting to re-initialize database. {e.Message}");
                db.Database.EnsureDeleted();
                db.Database.Migrate();
                logger.LogInformation("Database re-initialized.");
            }
        }

    }



    private static void Info(string tag, string message)
    {
#if ANDROID
        Log.Info(tag, message);
#endif
    }
}
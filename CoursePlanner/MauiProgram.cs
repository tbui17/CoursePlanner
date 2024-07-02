using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.Services;
using CoursePlanner.ViewModels;
using Lib;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

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

#if DEBUG
        builder
           .Logging
           .AddDebug();
#endif

        return builder.Build();
    }
}
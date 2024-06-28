using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.ViewModels;
using Lib;
using static CoursePlanner.Utils.MauiProviderExtensions;
using Microsoft.Extensions.Logging;

namespace CoursePlanner
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
               .UseMauiApp<App>()
               .UseMauiCommunityToolkit()
               .UseMauiCommunityToolkitMarkup()
               .ConfigureFonts
                (
                    fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    }
                );

            var services = builder.Services;
            Configs
               .ConfigBackendServices(services)
               .AddTransientWithShellRoute<MainPage, MainViewModel>()
               .AddSingleton<AppShellViewModel>()
               .AddSingletonWithShellRoute<DetailedTermPage, DetailedTermViewModel>()
               .AddTransientWithShellRoute<EditTermPage, EditTermViewModel>()
               .AddTransientWithShellRoute<DetailedCoursePage, DetailedCourseViewModel>()
               .AddTransientWithShellRoute<EditCoursePage, EditCourseViewModel>()
                
                ;


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
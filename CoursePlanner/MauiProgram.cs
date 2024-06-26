using CommunityToolkit.Maui;
using Lib;
using Microsoft.Extensions.Logging;
using ViewModels;

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
               .ConfigureFonts
                (
                    fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    }
                );

            var services = builder.Services;
            Configs.ConfigBackendServices(services);
            services.AddSingleton<MainPage>();
            services.AddSingleton<NewPage1>();
            services.AddSingleton<MainViewModel>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
using CommunityToolkit.Maui;
using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification;
using Serilog;
using Serilog.Formatting.Json;
using ViewModels.Config;
using ViewModels.Services;
using ViewModels.Setup;

namespace MauiConfig;


public class MauiConfigurationBuilder
{

    public MauiAppBuilder CreateBuilder()
    {
        var config = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(new JsonFormatter(), FileSystem.AppDataDirectory + "/logs/log.json", buffered: true)
            .Enrich.FromLogContext();

#if DEBUG
        config = config.MinimumLevel.Debug().WriteTo.Debug();
#elif RELEASE
        config = config.MinimumLevel.Information();
#endif

        Log.Logger = config.CreateLogger();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .ConfigureFonts
            (
                fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }
            );


        builder.Services
            .AddBackendServices()
            .AddServices()
            .AddAssemblyNames([nameof(CoursePlanner)])
            .AddDbContext<LocalDbCtx>(b =>
                {
                    b.UseSqlite($"DataSource={FileSystem.Current.AppDataDirectory}/database.db");
#if DEBUG
                    b.EnableSensitiveDataLogging();
                    b.EnableDetailedErrors();
#endif
                },
                ServiceLifetime.Transient
            )
            .AddDbContextFactory<LocalDbCtx>(lifetime: ServiceLifetime.Transient)
            .AddSingleton<IAppService, AppService>()
            .AddSingleton<AppShell>()
            .AddSingleton<ISessionService, SessionService>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddTransient<MainPage>()
            .AddTransient<DetailedTermPage>()
            .AddTransient<EditTermPage>()
            .AddTransient<DetailedCoursePage>()
            .AddTransient<EditCoursePage>()
            .AddTransient<EditNotePage>()
            .AddTransient<EditAssessmentPage>()
            .AddTransient<TermListView>()
            .AddTransient<DevPage>()
            .AddTransient<DbSetup>()
            .AddTransient<LoginView>()
            .AddTransient<NotificationDataPage>()
            .AddTransient<StatsPage>();

        return builder;
    }

}
using CommunityToolkit.Maui;
using CoursePlanner.Exceptions;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
using CoursePlanner.Views;
using MauiConfig;
using Plugin.LocalNotification;
using Serilog;
using UraniumUI;
using ViewModels.Services;

namespace CoursePlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var setup = new MauiAppBuilderFactory<App>
        {
            AssemblyName = nameof(CoursePlanner),
            AppDataDirectory = () => FileSystem.Current.AppDataDirectory,
            MainPage = () => Application.Current?.MainPage,
            ExceptionHandlerRegistration = x => MauiExceptions.UnhandledException += x,
            ServiceBuilderFactory = x => new MauiServiceBuilder(x),
        };

        var app = setup.CreateBuilder()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .Build();
        setup.RunStartupActions(app);

        return app;
    }
}

file class MauiServiceBuilder(MauiAppBuilder builder) : IMauiServiceBuilder
{
    public void AddViews()
    {
        builder.Services
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
    }

    public void AddAppServices()
    {
        builder.Services
            .AddSingleton<IAppService, AppService>()
            .AddSingleton<AppShell>()
            .AddSingleton<ISessionService, SessionService>()
            .AddSingleton<INavigationService, NavigationService>();
    }

    public void SetLogger(LoggerConfiguration logger)
    {
        Log.Logger = logger.CreateLogger();
    }

    public void AddMauiDependencies()
    {
        builder
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseUraniumUI()
            .UseUraniumUIMaterial();

    }
}
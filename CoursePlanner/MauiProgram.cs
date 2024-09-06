using CommunityToolkit.Maui;
using CoursePlanner.Exceptions;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Views;
using MauiConfig;
using Plugin.LocalNotification;
using UraniumUI;
using ViewModels.Services;

namespace CoursePlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseLocalNotification()
            .UseUraniumUI()
            .UseUraniumUIMaterial();
        var setup = new MauiAppServiceConfiguration
        {
            AppDataDirectory = () => FileSystem.Current.AppDataDirectory,
            MainPage = () => Application.Current?.MainPage,
            ExceptionHandlerRegistration = x => MauiExceptions.UnhandledException += x,
            Services = builder.Services,
            ServiceBuilder = new MauiServiceBuilder(builder.Services)
        };
        setup.ConfigServices();

        var app = builder.Build();
        setup.RunStartupActions(app);

        return app;
    }
}

file class MauiServiceBuilder(IServiceCollection services) : IMauiServiceBuilder
{
    public void AddViews()
    {
        services
            .AddTransient<MainPage>()
            .AddTransient<DetailedTermPage>()
            .AddTransient<EditTermPage>()
            .AddTransient<DetailedCoursePage>()
            .AddTransient<EditCoursePage>()
            .AddTransient<EditNotePage>()
            .AddTransient<EditAssessmentPage>()
            .AddTransient<TermListView>()
            .AddTransient<DevPage>()
            .AddTransient<LoginView>()
            .AddTransient<NotificationDataPage>()
            .AddTransient<StatsPage>();
    }

    public void AddAppServices()
    {
        services
            .AddSingleton<IAppService, AppService>()
            .AddSingleton<AppShell>()
            .AddSingleton<ISessionService, SessionService>()
            .AddSingleton<INavigationService, NavigationService>();
    }
}
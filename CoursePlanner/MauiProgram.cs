using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
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
            .UseMauiCommunityToolkitMarkup()
            .UseLocalNotification()
            .UseUraniumUI()
            .UseUraniumUIMaterial();
        var setup = new MauiAppServiceConfiguration
        {
            AppDataDirectory = () => FileSystem.Current.AppDataDirectory,
            MessageDisplayService = MessageDisplayService.Create(),
            ExceptionHandlerRegistration = x => MauiExceptions.UnhandledException += x,
            Services = builder.Services,
            ServiceBuilder = new MauiServiceBuilder(builder.Services)
        };
        setup.AddServices();

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
            .AddTransient<StatsPage>()
            .AddTransient<SettingsPage>();
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
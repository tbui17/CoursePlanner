using CoursePlanner.Exceptions;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
using CoursePlanner.Views;
using MauiConfig;
using Serilog;
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
            ServiceBuilderFactory = x => new MauiServiceBuilder(x.Services),
        };

        var app = setup.CreateBuilder().Build();
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
            .AddTransient<DbSetup>()
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

    public void SetLogger(LoggerConfiguration logger)
    {
        Log.Logger = logger.CreateLogger();
    }
}
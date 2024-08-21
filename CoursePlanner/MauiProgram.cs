using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
using CoursePlanner.Views;
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
            .AddKeyedTransient<MainPage>(nameof(NavigationTarget.TermListPage), (x, _) =>
            {
                var page = x.CreateInstance<MainPage>();
                var view = x.GetRequiredService<TermListView>();
                page.SetView(view);
                return page;


            })
            .AddTransient<DetailedTermPage, DetailedTermViewModel>()
            .AddTransient<EditTermPage, EditTermViewModel>()
            .AddTransient<DetailedCoursePage, DetailedCourseViewModel>()
            .AddTransient<EditCoursePage, EditCourseViewModel>()
            .AddTransient<InstructorFormPage, InstructorFormViewModel>()
            .AddTransient<EditNotePage, EditNoteViewModel>()
            .AddTransient<EditAssessmentPage, EditAssessmentViewModel>()
            .AddTransient<PageResolver>()
            .AddTransient<LoginViewModel>()
            .AddTransient<TermViewModel>()
            .AddTransient<TermListView>()
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

public static class ContainerExtensions
{
    public static T CreateInstance<T>(this IServiceProvider provider)
    {
        return ActivatorUtilities.CreateInstance<T>(provider);
    }

    public static IServiceCollection AddTransient2<T>(this IServiceCollection services, Func<AddTransientCtx<T>, T> fn)
        where T : class
    {
        services.AddTransient(provider =>
        {
            var instance = fn(new AddTransientCtx<T>(provider.CreateInstance<T>(), provider));
            return instance;
        });
        return services;
    }

    public record AddTransientCtx<T>(T Instance, IServiceProvider Provider)
    {
        public static implicit operator T(AddTransientCtx<T> ctx) => ctx.Instance;
    };
}
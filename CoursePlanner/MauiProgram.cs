﻿using CommunityToolkit.Maui;
using CoursePlanner.Pages;
using CoursePlanner.Services;
using CoursePlanner.Utils;
using CoursePlanner.Views;
using Lib;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using UraniumUI;
using ViewModels.Config;
using ViewModels.Services;

namespace CoursePlanner;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var app = CreateBuilder().Build();
        var services = app.Services;
        RunStartupActions();

        return app;

        void RunStartupActions()
        {
            services
                .GetRequiredService<DbSetup>()
                .SetupDb();

            services
                .GetRequiredService<RefreshableViewService>()
                .InitializeCache();
        }
    }

    private static MauiAppBuilder CreateBuilder()
    {
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


        Configs
            .ConfigBackendServices(builder.Services)
            .ConfigServices()
            .ConfigAssemblyNames([nameof(CoursePlanner)])
            .AddDbContext<LocalDbCtx>(b =>
                {
                    b
                        .UseSqlite($"DataSource={FileSystem.Current.AppDataDirectory}/database.db")
                        .LogTo(Console.WriteLine);
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
            .AddTransient<NotificationDataPage>();

        builder.Logging.AddConsole();

#if DEBUG
        builder
            .Logging
            .AddDebug();
#endif


        return builder;
    }
}
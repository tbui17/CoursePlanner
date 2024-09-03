using CommunityToolkit.Maui;
using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Json;
using ViewModels.Config;
using ViewModels.ExceptionHandlers;
using ViewModels.Services;
using ViewModels.Setup;

namespace MauiConfig;

public class MauiAppBuilderFactory<TApp> where TApp : class, IApplication
{

    public required Func<string> AppDataDirectory { get; set; }
    public required MainPageGetter MainPage { get; set; }
    public required string AssemblyName { get; set; }
    public required ServiceBuilderFactory ServiceBuilderFactory { get; set; }
    public required Action<UnhandledExceptionEventHandler> ExceptionHandlerRegistration { get; set; }


    public void RunStartupActions(MauiApp app)
    {
        var handler = app.Services.GetRequiredService<ClientExceptionHandler>();

        ExceptionHandlerRegistration((_, e) => handler.OnUnhandledException(e).Wait());
        var client = app.Services.GetRequiredService<SetupClient>();
        client.Setup();

    }
    public MauiAppBuilder CreateBuilder()
    {

        var config = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(new JsonFormatter(), AppDataDirectory() + "/logs/log.json", buffered: true)
            .Enrich.FromLogContext();

#if DEBUG
        config = config.MinimumLevel.Debug().WriteTo.Debug();
#elif RELEASE
        config = config.MinimumLevel.Information();
#endif

        var builder = MauiApp.CreateBuilder();
        var serviceBuilder = ServiceBuilderFactory(builder);
        serviceBuilder.SetLogger(config);

        builder
            .UseMauiApp<TApp>()
            .UseMauiCommunityToolkit()
            // .UseLocalNotification()
            // .UseUraniumUI()
            // .UseUraniumUIMaterial()
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
            .AddSingleton(MainPage)
            .AddAssemblyNames([AssemblyName])
            .AddDbContext<LocalDbCtx>(b =>
                {
                    b.UseSqlite($"DataSource={AppDataDirectory()}/database.db");
#if DEBUG
                    b.EnableSensitiveDataLogging();
                    b.EnableDetailedErrors();
#endif
                },
                ServiceLifetime.Transient
            )
            .AddSingleton<ISessionService, SessionService>()
            .AddDbContextFactory<LocalDbCtx>(lifetime: ServiceLifetime.Transient);

        serviceBuilder.AddViews();
        serviceBuilder.AddAppServices();

        return builder;
    }

}

public delegate IMauiServiceBuilder ServiceBuilderFactory(MauiAppBuilder builder);

public interface IMauiServiceBuilder
{
    void AddViews();
    void AddAppServices();
    void SetLogger(LoggerConfiguration logger);
    void AddMauiDependencies();
}

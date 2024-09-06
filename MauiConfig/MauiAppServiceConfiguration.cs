
using System.Text;
using Lib;
using Lib.Config;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using ViewModels.Config;
using ViewModels.ExceptionHandlers;
using ViewModels.Services;
using ViewModels.Setup;

namespace MauiConfig;

public class MauiAppServiceConfiguration
{

    public required IServiceCollection Services { get; set; }
    public required IMauiServiceBuilder ServiceBuilder { get; set; }
    public required Func<string> AppDataDirectory { get; set; }
    public required MainPageGetter MainPage { get; set; }
    public required Action<UnhandledExceptionEventHandler> ExceptionHandlerRegistration { get; set; }


    public void RunStartupActions(MauiApp app)
    {
        var handler = app.Services.GetRequiredService<ClientExceptionHandler>();

        ExceptionHandlerRegistration((_, e) => handler.OnUnhandledException(e).Wait());
        var client = app.Services.GetRequiredService<SetupClient>();
        client.Setup();

    }
    public IServiceCollection ConfigServices()
    {

        var logPath = Path.Combine(AppDataDirectory(),"logs","log.json");

        var config = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                formatter: new JsonFormatter(),
                path: logPath,
                restrictedToMinimumLevel: LogEventLevel.Information,
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: 3,
                rollOnFileSizeLimit: true,
                shared: true,
                buffered: true
                )
            .Enrich.FromLogContext();

#if DEBUG
        config = config.MinimumLevel.Debug().WriteTo.Debug();
#elif RELEASE
        config = config.MinimumLevel.Information();
#endif




        var serviceBuilder = ServiceBuilder;



        var vmConfig = new ViewModelConfig(Services);

        Services.AddBackendServices();
        vmConfig.AddServices();


        Services
            .AddSingleton(MainPage)
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

        return Services;
    }

}
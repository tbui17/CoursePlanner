
using Lib;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
    public required string AssemblyName { get; set; }
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

        var config = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(new JsonFormatter(), AppDataDirectory() + "/logs/log.json", buffered: true)
            .Enrich.FromLogContext();

#if DEBUG
        config = config.MinimumLevel.Debug().WriteTo.Debug();
#elif RELEASE
        config = config.MinimumLevel.Information();
#endif




        var serviceBuilder = ServiceBuilder;
        serviceBuilder.SetLogger(config);

        var assemblyService = new AssemblyService(AppDomain.CurrentDomain);
        var backendConfig = new BackendConfig(assemblyService, Services);
        var vmConfig = new ViewModelConfig(assemblyService, Services);

        backendConfig.AddBackendServices();
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

public interface IMauiServiceBuilder
{
    void AddViews();
    void AddAppServices();
    void SetLogger(LoggerConfiguration logger);
}

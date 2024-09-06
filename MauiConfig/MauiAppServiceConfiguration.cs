using Lib.Config;
using Lib.Models;
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

    public void AddServices()
    {
        var logPath = Path.Combine(AppDataDirectory(), "logs", "log.json");
        var fileSinkOptions = new FileSinkOptions
        {
            Formatter = new JsonFormatter(),
            Path = logPath,
            RestrictedToMinimumLevel = LogEventLevel.Information,
            RollingInterval = RollingInterval.Infinite,
            RetainedFileCountLimit = 3,
            RollOnFileSizeLimit = true,
            Shared = false,
            Buffered = true
        };

        var config = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(fileSinkOptions)
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
    }
}

public class MauiLoggingUseCase : ILoggingUseCase
{
    private readonly string _appDataDirectory;

    public MauiLoggingUseCase(string appDataDirectory)
    {
        _appDataDirectory = appDataDirectory;
        Base = new DefaultLogConfigurationUseCase() { Configuration = Configuration };
    }

    public LoggerConfiguration Configuration { get; set; } = new();
    private DefaultLogConfigurationUseCase Base { get; set; }

    public void SetMinimumLogLevel()
    {
#if DEBUG
        Configuration.MinimumLevel.Debug().WriteTo.Debug(LogEventLevel.Debug);
#elif RELEASE
         Configuration.MinimumLevel.Information();
#endif
    }

    public void AddSinks()
    {
        var opts = DefaultLogConfigurationUseCase.FileSinkOptions with
        {
            Formatter = new JsonFormatter(),
            Path = Path.Combine(_appDataDirectory, "logs", "log.json"),
            RestrictedToMinimumLevel = LogEventLevel.Information,
            RollingInterval = RollingInterval.Infinite,
            RetainedFileCountLimit = 3,
            RollOnFileSizeLimit = true,
            Shared = true,
            Buffered = true
        };
        Configuration.WriteTo.File(opts)
            .WriteTo.Console(LogEventLevel.Information);
    }

    public void AddEnrichments()
    {
        Base.AddEnrichments();
    }

    public void AddLogFilters()
    {
        Base.AddLogFilters();
    }
}
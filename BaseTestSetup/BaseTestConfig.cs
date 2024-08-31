using Serilog.Core;

namespace BaseTestSetup;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public static class BaseTestConfig
{
    public static IServiceCollection AddLogger(this IServiceCollection services, Action<Logger> setter)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Debug()
            .Enrich.FromLogContext()
            .CreateLogger();
        setter(logger);

        return services.AddSerilog().AddLogging();
    }

    public static IServiceCollection AddTestDatabase(this IServiceCollection services)
    {
        var guid = Guid.NewGuid().ToString();
        return services.AddDbContext<LocalDbCtx>(x => x
                .UseSqlite($"DataSource={guid}")
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
            )
            .AddDbContextFactory<LocalDbCtx>();
    }
}
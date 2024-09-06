using Lib.Config;

namespace BaseTestSetup;

using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public static class BaseTestConfig
{
    public static IServiceCollection AddLogger(this IServiceCollection services)
    {
        return services.AddLoggingUseCase(new LogConfigurationTestUseCase());
    }

    public static IServiceCollection AddTestDatabase(this IServiceCollection services, string? fileName = null)
    {
        var name = fileName ?? Guid.NewGuid().ToString();
        return services.AddDbContext<LocalDbCtx>(x => x
                .UseSqlite($"DataSource={name}")
            )
            .AddDbContextFactory<LocalDbCtx>();
    }

    public static async Task GlobalTearDown()
    {
        await Log.CloseAndFlushAsync();
    }
}
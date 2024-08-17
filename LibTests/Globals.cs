using Lib;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests;

public static class Globals
{
    private static IServiceProvider? _provider;
    public static IServiceProvider Provider => _provider ??= CreateProvider();


    private static IServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        Configs
           .ConfigBackendServices(services)
           .AddDbContext<LocalDbCtx>(x => x
               .UseSqlite("DataSource=database.db")
               .EnableDetailedErrors()
               .EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine)
            )
           .AddDbContextFactory<LocalDbCtx>()
           .AddTransient<NotificationSetupUtil>();
        return services.BuildServiceProvider();
    }
}
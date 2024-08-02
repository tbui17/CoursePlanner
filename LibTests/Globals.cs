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
        Configs.ConfigBackendServices(services);
        services.AddDbContextFactory<LocalDbCtx>(x => x.UseSqlite("Data Source=:memory:"));
        return services.BuildServiceProvider();
    }
}
using Lib;
using Lib.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTests;

public static class Globals
{
    private static IServiceProvider? _provider;
    public static IServiceProvider Provider => _provider ??= CreateProvider();


    private static IServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        Configs.ConfigBackendServices(services);
        return services.BuildServiceProvider();
    }
}
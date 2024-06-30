using Lib.Models;
using Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public class Configs
{
    public static IServiceCollection ConfigBackendServices(IServiceCollection b)
    {
        b
           .AddDbContextFactory<LocalDbCtx>()
           .AddScoped<NotificationService>();
        return b;
    }
}
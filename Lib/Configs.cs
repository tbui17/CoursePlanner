using Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public class Configs
{
    public static IServiceCollection ConfigBackendServices(IServiceCollection b)
    {
        b
           .AddScoped<NotificationService>();
        return b;
    }
}
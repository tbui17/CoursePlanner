using Lib.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Lib;

public class Configs
{

    public static IServiceCollection ConfigBackendServices(IServiceCollection b)
    {
        b.AddDbContextFactory<LocalDbCtx>();
        return b;
    }
}
using Lib;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

public abstract class BaseSetup
{
    protected IServiceProvider Provider;

    protected BaseSetup()
    {
        var service = new ServiceCollection();
        Configs.ConfigBackendServices(service);
        Provider = service.BuildServiceProvider();
    }
}
using Lib;
using Microsoft.Extensions.DependencyInjection;
using ViewModels;

namespace Tests;

public abstract class BaseSetup
{
    protected IServiceProvider Provider;

    protected BaseSetup()
    {
        var service = new ServiceCollection();
        Configs.ConfigBackendServices(service);
        service.AddSingleton<MainViewModel>();
        Provider = service.BuildServiceProvider();
    }
}
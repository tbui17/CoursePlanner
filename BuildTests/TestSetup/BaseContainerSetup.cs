using BuildLib.Utils;
using BuildTests.Utils;
using Serilog;
using Xunit.Abstractions;

namespace BuildTests.TestSetup;

public class BaseContainerSetup
{
    protected readonly Container Container;

    public BaseContainerSetup(ITestOutputHelper testOutputHelper)
    {
        Container = new ContainerInitializer().GetContainer();
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo
            .TestOutput(testOutputHelper)
            .CreateLogger();
    }

    public T Resolve<T>() where T : notnull => Container.Resolve<T>();
    public T GetConfiguration<T>() => Container.GetConfiguration<T>();
}
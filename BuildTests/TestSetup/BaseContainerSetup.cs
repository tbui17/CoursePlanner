using BuildLib.Secrets;
using BuildLib.Utils;
using BuildTests.Utils;
using Serilog;
using Xunit.Abstractions;

namespace BuildTests.TestSetup;

public abstract class BaseContainerSetup
{
    protected readonly Container Container;

    public BaseContainerSetup(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateLogger();
        Container = GetContainer();
    }

    private static Container GetContainer() => new ContainerInitializer().GetContainer();

    public T Resolve<T>() where T : notnull => Container.Resolve<T>();
    public T GetConfiguration<T>() => Container.GetConfiguration<T>();
    public AppConfiguration GetConfiguration() => GetConfiguration<AppConfiguration>();
}
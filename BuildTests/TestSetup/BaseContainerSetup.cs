using BuildLib.Logging;
using BuildLib.Utils;
using BuildTests.Utils;
using Serilog;
using Xunit.Abstractions;

namespace BuildTests.TestSetup;

public abstract class BaseContainerSetup : IAsyncDisposable
{
    protected readonly Container Container;

    protected BaseContainerSetup(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.TestOutput(testOutputHelper, LogUtil.DefaultExpressionTemplate)
            .CreateLogger();
        Container = GetContainer();
    }

    public async ValueTask DisposeAsync()
    {
        await OnDisposeAsync();
        await Log.CloseAndFlushAsync();
    }

    protected virtual ValueTask OnDisposeAsync()
    {
        return ValueTask.CompletedTask;
    }


    private static Container GetContainer() => new ContainerInitializer().GetContainer();

    public T Resolve<T>() where T : notnull => Container.Resolve<T>();
}
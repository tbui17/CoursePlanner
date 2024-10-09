using System.Diagnostics.CodeAnalysis;
using BuildLib.Clients;
using BuildLib.Secrets;
using BuildTests.Utils;
using Serilog;
using Xunit.Abstractions;
using Container = BuildLib.Utils.Container;

namespace BuildTests.BuildProject;

public sealed class BundleUploadTest : IAsyncDisposable
{
    private readonly Container _container;

    public BundleUploadTest(ITestOutputHelper testOutputHelper)
    {
        _container = new ContainerInitializer().GetContainer();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateLogger();
    }

    public async ValueTask DisposeAsync()
    {
        await Log.CloseAndFlushAsync();
    }

    [Fact(Skip = "This test is for manual use only.")]
    [SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped")]
    public async Task UploadBundle_Succeeds()
    {
        var client = _container.Resolve<AndroidPublisherClient>();
        var config = _container.GetConfiguration<CoursePlannerConfiguration>();
        var path = config.BundlePath;

        await using var stream = File.Open(path, FileMode.Open);
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        await client.UploadBundle(stream, cts.Token);
    }
}
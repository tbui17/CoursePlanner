using System.Diagnostics.CodeAnalysis;
using BuildLib.CloudServices.GooglePlay;
using BuildLib.Utils;
using BuildTests.Attributes;
using BuildTests.Utils;
using Nuke.Common.ProjectModel;
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

    [ManualTest]
    [SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped")]
    public async Task UploadBundle_Succeeds()
    {
        var client = _container.Resolve<AndroidPublisherClient>();
        var solution = _container.Resolve<Solution>();
        var path = solution.GetSignedAabFile();

        await using var stream = File.Open(path, FileMode.Open);
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(3));

        await client.UploadBundle(stream, cts.Token);
    }
}
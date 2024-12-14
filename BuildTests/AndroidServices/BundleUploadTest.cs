using BuildLib.Commands;
using BuildLib.SolutionBuild;
using BuildTests.Attributes;
using BuildTests.TestSetup;
using Xunit.Abstractions;

namespace BuildTests.AndroidServices;

public sealed class BundleUploadTest(ITestOutputHelper testOutputHelper) : BaseContainerSetup(testOutputHelper)
{
    [ManualTest]
    public async Task UploadBundle_Succeeds()
    {
        var service = Resolve<PublishService>();
        await service.UploadToGooglePlay();
    }

    [ManualTest]
    public async Task PublishUpload_Succeeds()
    {
        var cmd = Resolve<PublishCommand>();
        await cmd.ExecutePublishAndUploadAsync();
    }
}
using BuildLib.SolutionBuild;
using BuildLib.Utils;

namespace BuildLib.Commands;

[Inject]
public class PublishCommand(
    PublishService publishService
)
{
    public async Task ExecuteAsync()
    {
        await publishService.ExecuteDotNetPublish();
    }

    public async Task ExecuteUploadAsync()
    {
        await publishService.UploadToGooglePlay();
    }

    public async Task ExecutePublishAndUploadAsync()
    {
        await ExecuteAsync();
        await publishService.UploadToGooglePlay();
    }
}
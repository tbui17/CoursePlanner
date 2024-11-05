using BuildLib.Secrets;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.Commands;

[Inject]
public class PublishCommand(
    ILogger<PublishCommand> logger,
    PublishService publishService,
    IOptions<GooglePlayDeveloperApiConfiguration> options
)
{
    public async Task ExecuteAsync()
    {
        logger.LogDebug("Publishing {ProjectName}", options.Value.ProjectName);


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
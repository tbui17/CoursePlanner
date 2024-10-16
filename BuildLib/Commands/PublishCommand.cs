using BuildLib.Secrets;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuke.Common.ProjectModel;

namespace BuildLib.Commands;

[Inject]
public class PublishCommand(
    ILogger<PublishCommand> logger,
    PublishService publishService,
    IOptions<AppConfiguration> options,
    Solution solution
)
{
    public async Task ExecuteAsync()
    {
        logger.LogDebug("Publishing {ProjectName}", options.Value.ProjectName);


        await publishService.ExecuteDotNetPublish();
    }
}
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.Commands;

[Inject]
public class PublishCommand(
    ILogger<PublishCommand> logger,
    PublishService publishService,
    AppConfiguration configs
)
{
    public Task ExecuteAsync()
    {
        logger.LogInformation("Publishing {ProjectName}", configs.ProjectName);
        var settings = publishService.CreateDotNetPublishSettings(configs.SolutionName, configs.ProjectName);
        DotNetTasks.DotNetPublish(settings);
        return Task.CompletedTask;
    }
}
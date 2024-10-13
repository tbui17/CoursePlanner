using BuildLib.FileSystem;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.Commands;

[Inject]
public class PublishCommand(
    ILogger<PublishCommand> logger,
    PublishService publishService,
    DirectoryService directoryService,
    FileArgFactory fileArgFactory
)
{
    public Task ExecuteAsync(string projectName)
    {
        logger.LogInformation("Publishing {ProjectName}", projectName);


        var arg = fileArgFactory.CreateProject(projectName);
        var projectPath = directoryService.GetOrThrowProjectFile(arg);

        var settings =
            publishService.CreateDotNetPublishSettings(projectPath,
                publishService.CreateAndroidSigningKeyStoreOptions()
            );

        DotNetTasks.DotNetPublish(settings);

        return Task.CompletedTask;
    }
}

public record PublishCommandArgs
{
    public required string ProjectPath { get; init; }
    public required string Configuration { get; init; }
}
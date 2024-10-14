using BuildLib.Secrets;
using BuildLib.Utils;
using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.Commands;

[Inject]
public class PublishCommand(
    ILogger<PublishCommand> logger,
    PublishService publishService,
    IOptions<AppConfiguration> options,
    Solution solution
)
{
    public Task ExecuteAsync()
    {
        logger.LogInformation("Publishing {ProjectName}", options.Value.ProjectName);


        var settings = publishService.CreateDotNetPublishSettings();
        DotNetTasks.DotNetPublish(settings);
        MoveFiles();


        return Task.CompletedTask;
    }

    private void MoveFiles()
    {
        var file = solution.GetProject(options.Value.ProjectName)!
            .Directory.GlobFiles("bin/**/publish/*Signed.aab")
            .Single();

        var outputFolder = solution.Directory / "output";
        if (outputFolder.Exists())
        {
            logger.LogInformation("Deleting output folder");
        }

        outputFolder.CreateOrCleanDirectory();
        if (!outputFolder.Exists())
        {
            throw new Exception("Unexpected error");
        }

        logger.LogInformation("Copying {File} to {OutputFolder}", file, outputFolder);
        file.CopyToDirectory(outputFolder, ExistsPolicy.FileOverwriteIfNewer);

        var byteSize = ByteSize
            .FromBytes(new FileInfo(file).Length)
            .ToString(ByteSize.MegaByteSymbol);

        logger.LogInformation("Created {File} with size {Mb}", file, byteSize);
    }
}
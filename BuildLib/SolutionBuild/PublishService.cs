using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.SolutionBuild;

[Inject]
public class PublishService(
    ILogger<PublishService> logger,
    DotNetPublishSettings settings,
    Solution solution,
    ReleaseProject releaseProject,
    VersionService versionService
)
{
    public async Task ExecuteDotNetPublish()
    {
        await versionService.ValidateAppVersion();

        logger.LogInformation("Publishing project {ProjectPath} with configuration {Configuration}",
            settings.Project,
            settings.Configuration
        );

        DotNetTasks.DotNetPublish(settings);
        logger.LogInformation("Published project {ProjectPath} with configuration {Configuration}",
            settings.Project,
            settings.Configuration
        );
        MoveFiles();
    }

    private void MoveFiles()
    {
        var file = releaseProject
            .Value.Directory
            .GlobFiles("bin/**/publish/*Signed.aab")
            .Single();

        var outputFolder = solution.Directory / "output";
        if (outputFolder.Exists())
        {
            logger.LogDebug("Deleting output folder");
        }

        outputFolder.CreateOrCleanDirectory();

        logger.LogDebug("Copying {File} to {OutputFolder}", file, outputFolder);
        file.CopyToDirectory(outputFolder, ExistsPolicy.FileOverwriteIfNewer);

        var byteSize = ByteSize
            .FromBytes(new FileInfo(file).Length)
            .ToString(ByteSize.MegaByteSymbol);

        logger.LogDebug("Created {File} with size {Mb}", file, byteSize);
    }
}
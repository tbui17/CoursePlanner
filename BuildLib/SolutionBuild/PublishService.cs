using BuildLib.AndroidPublish;
using BuildLib.CloudServices.GooglePlay;
using BuildLib.Secrets;
using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using ByteSizeLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    VersionService versionService,
    AndroidPublisherClient androidPublisherClient,
    AndroidSigningKeyStoreOptions opts,
    IOptions<AppConfiguration> configs
)
{
    public async Task ExecuteDotNetPublish()
    {
        await versionService.ValidateAppVersion();
        var contents = Convert.FromBase64String(configs.Value.KeystoreContents);
        await File.WriteAllBytesAsync(opts.AndroidSigningKeyStore, contents);


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

    public async Task UploadToGooglePlay()
    {
        var file = solution.GetSignedAabFile();
        await using var stream = File.OpenRead(file);

        var cts = new CancellationTokenSource(1000 * 60 * 30);
        await androidPublisherClient.UploadBundle(stream, cts.Token);
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
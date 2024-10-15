using BuildLib.CloudServices.GooglePlay;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Semver;

namespace BuildLib.SolutionBuild;

[Inject]
public class PublishService(
    ILogger<PublishService> logger,
    [FromKeyedServices(nameof(AppConfiguration.AppVersion))]
    SemVersion appVersion,
    AndroidPublisherClient androidPublisherClient,
    IMsBuildProject msBuildProject,
    DotNetPublishOptionsFactory optionsFactory
)
{
    public DotNetPublishSettings CreateDotNetPublishSettings()
    {
        ValidateAppVersion().Wait();
        var settings = optionsFactory.Create().ToDotNetPublishSettings();
        // immutable; produces copy
        var logData = settings.ClearProcessEnvironmentVariables().ClearProperties();
        logger.LogInformation(
            "Created publish settings: {@PartialData} {PropertyKeys} {ProcessEnvironmentVariableCount}",
            logData,
            settings.Properties.Keys,
            settings.ProcessEnvironmentVariables.Count
        );
        return settings;
    }

    public async Task UpdateAppVersion()
    {
        var latestVersionCode = await androidPublisherClient.GetLatestVersionCode();
        UpdateAppVersion(latestVersionCode);
    }

    public void UpdateAppVersion(int versionCode)
    {
        msBuildProject.ChangeAppDisplayVersion(appVersion);
        msBuildProject.ChangeAppVersionCode(versionCode);
    }

    public async Task ValidateAppVersion()
    {
        var latestVersionCode = await androidPublisherClient.GetLatestVersionCode();
        ValidateAppVersion(latestVersionCode);
    }

    public void ValidateAppVersion(int versionCode)
    {
        var data = msBuildProject.GetProjectVersionData();
        if (data.VersionCode - 1 != versionCode)
        {
            throw new InvalidDataException(
                $"Version code mismatch: expected {versionCode + 1} but found {data.VersionCode}"
            );
        }

        if (data.AppVersion != appVersion)
        {
            throw new InvalidDataException(
                $"App version mismatch: expected {appVersion} but found {data.AppVersion}"
            );
        }
    }

    public async Task ExecuteDotNetPublish()
    {
        var settings = CreateDotNetPublishSettings();
        var latestVersionCode = await androidPublisherClient.GetLatestVersionCode();
        ValidateAppVersion(latestVersionCode);
        logger.LogInformation("Publishing project {ProjectPath} with configuration {Configuration}",
            settings.Project,
            settings.Configuration
        );

        DotNetTasks.DotNetPublish(settings);
        logger.LogInformation("Published project {ProjectPath} with configuration {Configuration}",
            settings.Project,
            settings.Configuration
        );
    }
}
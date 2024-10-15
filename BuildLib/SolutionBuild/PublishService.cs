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
    IMsBuildProject msBuildProject,
    DotNetPublishOptionsFactory optionsFactory
)
{
    public DotNetPublishSettings CreateDotNetPublishSettings()
    {
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

    public void UpdateAppVersion()
    {
        var currentVersion = msBuildProject.GetAppVersion();
        if (currentVersion == appVersion)
        {
            logger.LogInformation("Current version {CurrentVersion} matches app version {AppVersion}, skipping update",
                currentVersion,
                appVersion
            );
            return;
        }

        logger.LogInformation("Updating current version {CurrentVersion} to app version {AppVersion}",
            currentVersion,
            appVersion
        );

        msBuildProject.ChangeAppVersion(appVersion);
        logger.LogInformation("Updated current version to app version");
    }

    public DotNetPublishSettings ExecuteDotNetPublish()
    {
        var settings = CreateDotNetPublishSettings();
        UpdateAppVersion();
        logger.LogInformation("Publishing project {ProjectPath} with configuration {Configuration}",
            settings.Project,
            settings.Configuration
        );
        DotNetTasks.DotNetPublish(settings);
        logger.LogInformation("Published project {ProjectPath} with configuration {Configuration}",
            settings.Project,
            settings.Configuration
        );
        return settings;
    }
}
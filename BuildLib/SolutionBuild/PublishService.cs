using System.Diagnostics;
using BuildLib.AndroidPublish;
using BuildLib.Logging;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.SolutionBuild;

[Inject]
public class PublishService(
    ILogger<PublishService> logger,
    ProcessLogger<PublishService> processLogger,
    ReleaseProject project,
    IOptions<AppConfiguration> configs,
    MsBuildProject msBuildProject
)
{
    public void UpdateAppVersion()
    {
    }

    public DotNetPublishSettings CreateDotNetPublishSettings()
    {
        var projectFile = new FileInfo(project.Value.Path);
        if (!projectFile.Exists)
        {
            throw new InvalidDataException($"Unexpected missing file {projectFile.FullName}");
        }

        var androidOptions = CreateAndroidSigningKeyStoreOptions();
        var settings = CreateDotNetPublishSettings(projectFile, androidOptions);
        return settings;
    }


    private DotNetPublishSettings CreateDotNetPublishSettings(
        FileInfo projectFile,
        AndroidSigningKeyStoreOptions properties
    )
    {
        if (!projectFile.Exists)
        {
            throw new InvalidDataException($"Unexpected missing file {projectFile.FullName}");
        }

        if (projectFile.Directory is null)
        {
            throw new InvalidDataException($"Unexpected missing directory for {projectFile.FullName}");
        }

        var settings = new DotNetPublishSettings()
            .EnableNoLogo()
            .SetProject(projectFile.FullName)
            .SetConfiguration(configs.Value.PublishConfiguration)
            .SetProcessLogger(processLogger.Log)
            .SetFramework(configs.Value.AndroidFramework);

        logger.LogDebug("Created publish settings: {@Data}", settings.ClearProcessEnvironmentVariables());
        settings = settings.SetProperties(properties.ToPropertyDictionary());
        logger.LogDebug("Set {Count} Android properties: {PropertyKeys}",
            settings.Properties.Count,
            settings.Properties.Keys
        );

        var value = settings.Properties[nameof(AndroidSigningKeyStoreOptions.AndroidKeyStore)];
        if (value is not bool v)
        {
            throw new UnreachableException($"Unexpected value type for AndroidKeyStore: ${value.GetType()}");
        }

        logger.LogDebug("AndroidKeyStore value: {AndroidKeyStore}", v);
        return settings;
    }

    private AndroidSigningKeyStoreOptions CreateAndroidSigningKeyStoreOptions()
    {
        logger.LogDebug("Retrieving Android properties from environment variables");
        var opts = new AppBuildOptions
        {
            AndroidSigningKeyAlias = Environment.GetEnvironmentVariable("COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS"),
            AndroidSigningKeyPass = Environment.GetEnvironmentVariable("COURSEPLANNER_KEY"),
            AndroidSigningKeyStore = Environment.GetEnvironmentVariable("COURSEPLANNER_ANDROID_SIGNING_KEY_STORE"),
            AndroidSigningStorePass = Environment.GetEnvironmentVariable("COURSEPLANNER_KEY"),
        }.ToValidatedAndroidSigningKeyStoreOptions();
        logger.LogDebug("Validated Android properties");
        return opts;
    }
}
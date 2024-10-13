using System.Diagnostics;
using BuildLib.AndroidPublish;
using BuildLib.FileSystem;
using BuildLib.Logging;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.Commands;

[Inject]
public class PublishService(
    ILogger<PublishService> logger,
    ProcessLogger<PublishService> processLogger,
    FileArgFactory fileArgFactory,
    DirectoryService directoryService
)
{
    public DotNetPublishSettings CreateDotNetPublishSettings(string solutionName, string projectName)
    {
        if (!solutionName.EndsWith(".sln"))
        {
            logger.LogInformation("Appending .sln to solution name");
            solutionName += ".sln";
        }

        if (projectName.EndsWith(".csproj"))
        {
            logger.LogInformation("Removing .csproj to project name");
            projectName = projectName.Replace(".csproj", "");
        }


        var solutionArgs = fileArgFactory.CreateSolution(solutionName);
        var solutionFile = directoryService.GetOrThrowSolutionFile(solutionArgs);
        var solution = SolutionModelTasks.ParseSolution(solutionFile.FullName);
        logger.LogInformation("Parsed solution: {Solution}", solution);
        var project = solution.GetProject(projectName).NotNull();
        var projectFile = new FileInfo(project.Path);
        if (!projectFile.Exists)
        {
            throw new InvalidDataException($"Unexpected missing file {projectFile.FullName}");
        }

        var androidOptions = CreateAndroidSigningKeyStoreOptions();
        var settings = CreateDotNetPublishSettings(projectFile, androidOptions);
        return settings;
    }


    public DotNetPublishSettings CreateDotNetPublishSettings(
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
            .SetConfiguration("Release")
            .SetProcessLogger(processLogger.Log)
            .SetFramework("net8.0-android");

        logger.LogInformation("Created publish settings: {@Data}", settings.ClearProcessEnvironmentVariables());
        settings = settings.SetProperties(properties.ToPropertyDictionary());
        logger.LogInformation("Set {Count} Android properties: {PropertyKeys}",
            settings.Properties.Count,
            settings.Properties.Keys
        );

        var value = settings.Properties[nameof(AndroidSigningKeyStoreOptions.AndroidKeyStore)];
        if (value is not bool v)
        {
            throw new UnreachableException($"Unexpected value type for AndroidKeyStore: ${value.GetType()}");
        }

        logger.LogInformation("AndroidKeyStore value: {AndroidKeyStore}", v);
        return settings;
    }

    public AndroidSigningKeyStoreOptions CreateAndroidSigningKeyStoreOptions()
    {
        logger.LogInformation("Retrieving Android properties from environment variables");
        var opts = new AppBuildOptions
        {
            AndroidSigningKeyAlias = Environment.GetEnvironmentVariable("COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS"),
            AndroidSigningKeyPass = Environment.GetEnvironmentVariable("COURSEPLANNER_KEY"),
            AndroidSigningKeyStore = Environment.GetEnvironmentVariable("COURSEPLANNER_ANDROID_SIGNING_KEY_STORE"),
            AndroidSigningStorePass = Environment.GetEnvironmentVariable("COURSEPLANNER_KEY"),
        }.ToValidatedAndroidSigningKeyStoreOptions();
        logger.LogInformation("Validated Android properties");
        return opts;
    }
}
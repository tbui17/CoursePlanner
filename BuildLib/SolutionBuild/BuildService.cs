using BuildLib.Utils;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;
using Semver;
using Serilog;
using Project = Microsoft.Build.Evaluation.Project;

namespace BuildLib.SolutionBuild;

// initialization must be done in two separate methods to avoid using conflicting types during initialization
// https://learn.microsoft.com/en-us/visualstudio/msbuild/find-and-use-msbuild-versions?view=vs-2022#register-instance-before-calling-msbuild
[Inject(Lifetime = ServiceLifetime.Singleton)]
public class MsBuildService(ReleaseProject project, ILogger<MsBuildProject> logger)
{
    private static bool Initialized { get; set; }

    public static void Initialize()
    {
        var log = Log.ForContext<MsBuildService>();
        if (Initialized)
        {
            log.Debug("Already initialized, skipping");
            return;
        }

        var instance = MSBuildLocator.QueryVisualStudioInstances().MaxBy(x => x.Version) ??
                       throw new InvalidDataException("No MSBuild instances found");
        log.Information("Found MSBuild instance {@Instance}, registering instance", instance);
        MSBuildLocator.RegisterInstance(instance);
        Initialized = true;
    }

    public MsBuildProject GetMsBuildProject() => new(project: project.Value.GetMSBuildProject(), logger: logger);
}

public class MsBuildProject(Project project, ILogger<MsBuildProject> logger)
{
    private const string ApplicationVersion = "ApplicationVersion";

    public SemVersion GetAppVersion()
    {
        var property = project.GetProperty(ApplicationVersion);
        if (property is null)
        {
            throw new InvalidDataException($"Missing {ApplicationVersion} property in project file {project.FullPath}");
        }


        if (!SemVersion.TryParse(property.EvaluatedValue, SemVersionStyles.Strict, out var version))
        {
            throw new InvalidDataException(
                $"Invalid {ApplicationVersion} property value {property.EvaluatedValue} could not be parsed into {typeof(Version)} in project file {project.FullPath}"
            );
        }

        logger.LogDebug("Found {ApplicationVersion} property with value {Version}", ApplicationVersion, version);

        return version;
    }

    public void SetAppVersion(Version version)
    {
        project.SetProperty(ApplicationVersion, version.ToString());
        project.Save();
    }
}
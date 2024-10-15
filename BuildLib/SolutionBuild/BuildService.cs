using BuildLib.Utils;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;
using Serilog;

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
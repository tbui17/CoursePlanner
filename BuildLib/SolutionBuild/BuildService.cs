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
        try
        {
            MSBuildLocator.RegisterInstance(instance);
        }
        catch (InvalidOperationException e) when (e.IsAlreadyLoadedError())
        {
            log.Information("MSBuild assemblies were already loaded, skipping initialization");
        }

        Initialized = true;
    }

    public MsBuildProject GetMsBuildProject()
    {
        var tries = 0;
        while (true)
        {
            try
            {
                tries++;
                var buildProj = project.Value.GetMSBuildProject();
                return new MsBuildProject(project: buildProj, logger: logger);
            }
            catch (FileLoadException e) when (e.Message.Contains("Could not load file or assembly") && tries < 3)
            {
                logger.LogWarning(e, "Failed to load MSBuild project, retrying. Attempt: {Tries}", tries);
            }
        }
    }
}

file static class InvalidOperationExtensions
{
    public static bool IsAlreadyLoadedError(this InvalidOperationException e)
    {
        const string message =
            "Microsoft.Build.Locator.MSBuildLocator.RegisterInstance was called, but MSBuild assemblies were already loaded.";

        return e.Message.ContainsIgnoreCase(message);
    }
}
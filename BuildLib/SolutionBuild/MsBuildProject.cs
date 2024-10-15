using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;
using Semver;

namespace BuildLib.SolutionBuild;

public interface IMsBuildProject
{
    SemVersion GetAppVersion();
    void ChangeAppVersion(SemVersion version);
}

public class MsBuildProject(Project project, ILogger<MsBuildProject> logger) : IMsBuildProject
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

    public void ChangeAppVersion(SemVersion version)
    {
        project.SetProperty(ApplicationVersion, version.ToString());
        project.Save();
    }
}
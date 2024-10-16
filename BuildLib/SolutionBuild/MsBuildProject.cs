using BuildLib.SolutionBuild.Versioning;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;
using Semver;

namespace BuildLib.SolutionBuild;

public interface IMsBuildProject
{
    SemVersion GetAppDisplayVersion();
    void ChangeAppDisplayVersion(SemVersion version);
    int GetAppVersionCode();
    void ChangeAppVersionCode(int versionCode);
    ProjectVersionData GetProjectVersionData();
}

public class MsBuildProject(Project project, ILogger<MsBuildProject> logger) : IMsBuildProject
{
    private const string ApplicationDisplayVersion = "ApplicationDisplayVersion";
    private const string ApplicationVersion = "ApplicationVersion";

    public SemVersion GetAppDisplayVersion()
    {
        var property = project.GetProperty(ApplicationDisplayVersion);
        if (property is null)
        {
            throw new InvalidDataException(
                $"Missing {ApplicationDisplayVersion} property in project file {project.FullPath}"
            );
        }


        if (!SemVersion.TryParse(property.EvaluatedValue, SemVersionStyles.Strict, out var version))
        {
            throw new InvalidDataException(
                $"Invalid {ApplicationDisplayVersion} property value {property.EvaluatedValue} could not be parsed into {typeof(Version)} in project file {project.FullPath}"
            );
        }

        logger.LogDebug("Found {ApplicationVersion} property with value {Version}", ApplicationDisplayVersion, version);

        return version;
    }

    public int GetAppVersionCode()
    {
        var property = project.GetProperty(ApplicationVersion);
        if (property is null)
        {
            throw new InvalidDataException($"Missing {ApplicationVersion} property in project file {project.FullPath}");
        }

        return int.Parse(property.EvaluatedValue);
    }

    public void ChangeAppVersionCode(int versionCode)
    {
        var prev = GetAppVersionCode();
        logger.LogInformation("Changing {ApplicationVersion} from {PreviousVersion} to {NewVersion}",
            ApplicationVersion,
            prev,
            versionCode
        );
        project.SetProperty(ApplicationVersion, versionCode.ToString()).Project.Save();
    }

    public ProjectVersionData GetProjectVersionData() =>
        new()
        {
            AppVersion = GetAppDisplayVersion(),
            VersionCode = GetAppVersionCode()
        };

    public void ChangeAppDisplayVersion(SemVersion version)
    {
        var prev = GetAppDisplayVersion();
        logger.LogInformation("Changing {ApplicationDisplayVersion} from {PreviousVersion} to {NewVersion}",
            ApplicationDisplayVersion,
            prev,
            version
        );
        project.SetProperty(ApplicationDisplayVersion, version.ToString()).Project.Save();
    }
}
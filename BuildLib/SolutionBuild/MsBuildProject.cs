using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;
using Semver;

namespace BuildLib.SolutionBuild;

public interface IMsBuildProject
{
    SemVersion GetAppDisplayVersion();
    string GetAppId();
    ProjectVersionData GetProjectVersionData();
    void SetVersion(ProjectVersionData versionData);
    void IncrementPatch();
    void IncrementMinor();
    void IncrementMajor();
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

    public string GetAppId()
    {
        return project.GetPropertyValue("ApplicationId") ??
               throw new InvalidDataException("Missing ApplicationId property in project file");
    }

    public ProjectVersionData GetProjectVersionData() =>
        new()
        {
            AppVersion = GetAppDisplayVersion(),
            VersionCode = GetAppVersionCode()
        };

    public void SetVersion(ProjectVersionData versionData)
    {
        var current = GetProjectVersionData();
        logger.LogInformation("Changing version data from {CurrentVersion} to {NewVersion}",
            current,
            versionData
        );
        if (versionData < current)
        {
            var message = new
            {
                Message = "Cannot change version to a lower version",
                CurrentVersion = current,
                NewVersion = versionData
            };
            throw new InvalidOperationException(message.ToString());
        }

        project.SetProperty(ApplicationDisplayVersion, versionData.AppVersion.ToString());
        project.SetProperty(ApplicationVersion, versionData.VersionCode.ToString());
        project.Save();
    }

    public void IncrementPatch()
    {
        var current = GetProjectVersionData();
        var newVersion = new ProjectVersionData
        {
            AppVersion = current.AppVersion.IncrementPatch(),
            VersionCode = current.VersionCode + 1
        };

        SetVersion(newVersion);
    }

    public void IncrementMinor()
    {
        var current = GetProjectVersionData();
        var newVersion = new ProjectVersionData
        {
            AppVersion = current.AppVersion.IncrementMinor(),
            VersionCode = current.VersionCode + 1
        };

        SetVersion(newVersion);
    }

    public void IncrementMajor()
    {
        var current = GetProjectVersionData();
        var newVersion = new ProjectVersionData
        {
            AppVersion = current.AppVersion.IncrementMajor(),
            VersionCode = current.VersionCode + 1
        };

        SetVersion(newVersion);
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
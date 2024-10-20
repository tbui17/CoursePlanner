using BuildLib.Utils;

namespace BuildLib.SolutionBuild.Versioning;

public interface IVersionService
{
    Task<ValidatedProjectVersionData> GetValidatedProjectVersionData();
    Task ValidateAppVersion();
}

[Inject(typeof(IVersionService))]
public class VersionService(
    IMsBuildProject msBuildProject,
    LatestAndroidServiceVersionProvider provider,
    ProjectVersionDataMapper mapper) : IVersionService
{
    public async Task<ValidatedProjectVersionData> GetValidatedProjectVersionData()
    {
        var latestVersionCode = await provider.GetLatestVersionCode();
        ValidateAppVersion(latestVersionCode);
        return msBuildProject
            .GetProjectVersionData()
            .Thru(mapper.ToValidatedProjectVersionData);
    }

    public async Task ValidateAppVersion()
    {
        var latestVersionCode = await provider.GetLatestVersionCode();
        ValidateAppVersion(latestVersionCode);
    }

    private void ValidateAppVersion(int versionCode)
    {
        var data = msBuildProject.GetProjectVersionData();
        var nextVersion = data.VersionCode + 1;
        if (versionCode != nextVersion)
        {
            throw new InvalidDataException(
                $"Version code mismatch: expected {versionCode + 1} but found {data.VersionCode}"
            );
        }
    }
}
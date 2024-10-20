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
        var data = msBuildProject.GetProjectVersionData();
        await ValidateAppVersion(data.VersionCode);
        return mapper.ToValidatedProjectVersionData(data);
    }

    public async Task ValidateAppVersion()
    {
        var projectCurrentVersionCode = msBuildProject.GetProjectVersionData().VersionCode;
        await ValidateAppVersion(projectCurrentVersionCode);
    }

    private async Task ValidateAppVersion(int projectCurrentVersionCode)
    {
        var latestVersionCode = await provider.GetLatestVersionCode();
        var expectedVersionCode = latestVersionCode + 1;

        if (projectCurrentVersionCode != expectedVersionCode)
        {
            throw new InvalidDataException(
                $"Version code mismatch: expected {expectedVersionCode} but found {projectCurrentVersionCode}"
            );
        }
    }
}
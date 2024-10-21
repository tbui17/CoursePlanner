using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace BuildLib.SolutionBuild.Versioning;

public interface IVersionService
{
    Task<ValidatedProjectVersionData> GetValidatedProjectVersionData();
    Task<ProjectVersionData> GetProjectVersionData();
    Task ValidateAppVersion();
}

[Inject(typeof(IVersionService), Lifetime = ServiceLifetime.Singleton)]
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

    public Task<ProjectVersionData> GetProjectVersionData()
    {
        return Task.FromResult(msBuildProject.GetProjectVersionData());
    }

    public async Task ValidateAppVersion()
    {
        var versionData = await GetValidatedProjectVersionData();
        var projectCurrentVersionCode = versionData.VersionCode;
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
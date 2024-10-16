using BuildLib.CloudServices.GooglePlay;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Semver;

namespace BuildLib.SolutionBuild.Versioning;

[Inject]
public class VersionService(
    IMsBuildProject msBuildProject,
    [FromKeyedServices(nameof(AppConfiguration.AppVersion))]
    SemVersion appVersion,
    AndroidPublisherClient androidPublisherClient,
    ProjectVersionDataMapper mapper)
{
    public async Task UpdateAppVersion()
    {
        var latestVersionCode = await androidPublisherClient.GetLatestVersionCode();
        UpdateAppVersion(latestVersionCode);
    }

    private void UpdateAppVersion(int versionCode)
    {
        msBuildProject.ChangeAppDisplayVersion(appVersion);
        msBuildProject.ChangeAppVersionCode(versionCode);
    }

    public async Task<ValidatedProjectVersionData> GetValidatedProjectVersionData()
    {
        var latestVersionCode = await androidPublisherClient.GetLatestVersionCode();
        ValidateAppVersion(latestVersionCode);
        return msBuildProject
            .GetProjectVersionData()
            .Thru(mapper.ToValidatedProjectVersionData);
    }

    public async Task ValidateAppVersion()
    {
        var latestVersionCode = await androidPublisherClient.GetLatestVersionCode();
        ValidateAppVersion(latestVersionCode);
    }

    private void ValidateAppVersion(int versionCode)
    {
        var data = msBuildProject.GetProjectVersionData();
        if (data.VersionCode - 1 != versionCode)
        {
            throw new InvalidDataException(
                $"Version code mismatch: expected {versionCode + 1} but found {data.VersionCode}"
            );
        }

        if (data.AppVersion != appVersion)
        {
            throw new InvalidDataException(
                $"App version mismatch: expected {appVersion} but found {data.AppVersion}"
            );
        }
    }
}
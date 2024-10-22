using BuildLib.CloudServices.GooglePlay;
using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildLib.SolutionBuild.Versioning;

public interface IVersionService
{
    Task<ProjectVersionData> GetProjectVersionData();
    Task ValidateAppVersion();
}

[Inject(typeof(IVersionService), Lifetime = ServiceLifetime.Singleton)]
public class VersionService(
    IMsBuildProject msBuildProject,
    ITrackService trackService,
    ILogger<VersionService> logger) : IVersionService
{
    private const long DefaultVersionCode = 1;

    public Task<ProjectVersionData> GetProjectVersionData()
    {
        return Task.FromResult(msBuildProject.GetProjectVersionData());
    }

    public async Task ValidateAppVersion()
    {
        var versionData = await GetProjectVersionData();
        var projectCurrentVersionCode = versionData.VersionCode;
        await ValidateAppVersion(projectCurrentVersionCode);
    }

    public async Task<long> GetLatestVersionCode()
    {
        var track = await trackService.GetTrack();

        logger.LogDebug("Received track: {@Track}", track);
        var versionCodes = track
            .Releases
            .SelectMany(x => x.VersionCodes)
            .OfType<long>()
            .ToList();

        if (versionCodes.Count is 0)
        {
            logger.LogInformation(
                "No version codes found in track {@Track}, assuming this is first upload. Returning default version code: {DefaultVersionCode}",
                track,
                DefaultVersionCode
            );
            return DefaultVersionCode;
        }

        return versionCodes.Max();
    }

    private async Task ValidateAppVersion(int projectCurrentVersionCode)
    {
        var latestVersionCode = await GetLatestVersionCode();
        var ctx = new AppVersionValidationContext(projectCurrentVersionCode, latestVersionCode);
        using var _ = logger.MethodScope();
        logger.LogDebug("Validating app version: {ValidationContext}", ctx);

        ctx.Validate();
    }

    public async Task IncrementVersion()
    {
    }
}

file record struct AppVersionValidationContext(
    int ProjectCurrentVersionCode,
    long LatestVersionCode
)
{
    public long ExpectedVersionCode => LatestVersionCode + 1;

    public void Validate()
    {
        if (ProjectCurrentVersionCode != ExpectedVersionCode)
        {
            throw new InvalidDataException(
                $"Version code mismatch: expected {ExpectedVersionCode} but found {ProjectCurrentVersionCode}"
            );
        }
    }
}
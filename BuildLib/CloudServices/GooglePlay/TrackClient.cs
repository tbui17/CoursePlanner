using BuildLib.Secrets;
using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

[Inject]
public class TrackClient(
    IOptions<AppConfiguration> configs,
    AndroidPublisherService service,
    ILogger<TrackClient> logger,
    VersionService versionService)
{
    public async Task<Track> GetTrack(string editId)
    {
        var res = await service
            .Edits.Tracks
            .Get(configs.Value.ApplicationId, editId, configs.Value.ReleaseTrack)
            .ExecuteAsync();

        logger.LogDebug("Received track: {@Track}", res);
        return res;
    }

    public async Task<Track> UpdateTrack(string editId)
    {
        var track = await GetTrack(editId);
        logger.LogDebug("Updating track: {@Track}", track);
        var versionData = await versionService.GetValidatedProjectVersionData();

        var release = track.Releases[0];
        release.Name = configs.Value.AppVersion;
        release.Status = configs.Value.ReleaseStatus;
        release.VersionCodes = [versionData.VersionCode];

        logger.LogDebug("Updated track: {@Track}", track);

        var res = await service
            .Edits.Tracks
            .Update(track, configs.Value.ApplicationId, editId, configs.Value.ReleaseTrack)
            .ExecuteAsync();

        logger.LogDebug("Saved changes to track: {@Track}", res);
        return res;
    }
}
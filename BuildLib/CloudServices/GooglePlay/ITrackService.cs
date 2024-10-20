using BuildLib.Secrets;
using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface ITrackService
{
    Task<Track> GetTrack(string editId);
    Task<Track> UpdateTrack(string editId, CancellationToken token = default);
}

[Inject(typeof(ITrackService))]
public class TrackService(
    IOptions<AppConfiguration> configs,
    AndroidPublisherService service,
    ILogger<ITrackService> logger,
    IVersionService versionService
) : ITrackService
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

    public async Task<Track> UpdateTrack(string editId, CancellationToken token = default)
    {
        var track = await GetTrack(editId);
        logger.LogDebug("Updating track: {@Track}", track);
        var versionData = await versionService.GetValidatedProjectVersionData();
        track.Releases =
        [
            new()
            {
                Name = versionData.AppVersion.ToString(),
                Status = configs.Value.ReleaseStatus,
                VersionCodes = [versionData.VersionCode]
            }
        ];

        logger.LogDebug("Updated track: {@Track}", track);

        var res = await service
            .Edits.Tracks
            .Update(track, configs.Value.ApplicationId, editId, configs.Value.ReleaseTrack)
            .ExecuteAsync(token);

        logger.LogDebug("Saved changes to track: {@Track}", res);
        return res;
    }
}
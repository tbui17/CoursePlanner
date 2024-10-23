using BuildLib.Secrets;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;

namespace BuildLib.CloudServices.GooglePlay;

public interface ITrackService
{
    public Task<Track> GetTrack(CancellationToken token = default);
    Task<Track> UpdateTrackFromConfiguration(CancellationToken token = default);
    Task<Track> RemoveReleases(Func<TrackRelease, bool> predicate, CancellationToken token = default);
}

[Inject(typeof(ITrackService), Lifetime = ServiceLifetime.Singleton)]
public class TrackService(
    IOptions<AppConfiguration> configs,
    AndroidPublisherService service,
    ILogger<ITrackService> logger,
    IEditProvider editProvider,
    IMsBuildProject msBuildProject
) : ITrackService
{
    public async Task<Track> GetTrack(CancellationToken token)
    {
        var res = await service
            .Edits.Tracks
            .Get(configs.Value.ApplicationId, editProvider.EditId, configs.Value.ReleaseTrack)
            .ExecuteAsync(token);

        logger.LogDebug("Received track: {@Track}", res);
        return res;
    }

    public async Task<Track> UpdateTrackFromConfiguration(CancellationToken token = default)
    {
        var track = CreateTrackFromConfiguration();

        logger.LogInformation("Created track update data: {@Track}", track);

        return await UpdateTrack(track, token);
    }

    public async Task<Track> RemoveReleases(Func<TrackRelease, bool> predicate, CancellationToken token = default)
    {
        using var _ = logger.MethodScope();
        var track = await GetTrack(token);
        var (toRemove, remaining) = track.Releases.Partition(predicate).ToLists();
        logger.LogInformation("Removing release. Diff: {@Remove} {@Keep}", toRemove, remaining);

        track.Releases = remaining;

        return await UpdateTrack(track, token);
    }

    private TrackRelease CreateTrackReleaseFromConfiguration()
    {
        var versionData = msBuildProject.GetProjectVersionData();


        var release = new TrackRelease
        {
            Name = versionData.AppVersion.ToString(),
            Status = configs.Value.ReleaseStatus,
            VersionCodes = [versionData.VersionCode],
        };
        return release;
    }

    private Track CreateTrackFromConfiguration()
    {
        var track = new Track
        {
            Releases = [CreateTrackReleaseFromConfiguration()]
        };

        return track;
    }


    public async Task<Track> UpdateTrack(Track track, CancellationToken token = default)
    {
        using var _ = logger.MethodScope();
        var res = await service
            .Edits.Tracks
            .Update(
                track,
                configs.Value.ApplicationId,
                editProvider.EditId,
                configs.Value.ReleaseTrack
            )
            .ExecuteAsync(token);


        logger.LogInformation("Saved changes to track: {@Track}", res);
        return res;
    }
}
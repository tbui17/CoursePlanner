using BuildLib.Secrets;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface ITrackService
{
    public Task<Track> GetTrack();
    Task<Track> UpdateTrack(string editId, CancellationToken token = default);
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
    public async Task<Track> GetTrack()
    {
        var res = await service
            .Edits.Tracks
            .Get(configs.Value.ApplicationId, editProvider.EditId, configs.Value.ReleaseTrack)
            .ExecuteAsync();

        logger.LogDebug("Received track: {@Track}", res);
        return res;
    }

    public async Task<Track> UpdateTrack(string editId, CancellationToken token = default)
    {
        var versionData = msBuildProject.GetProjectVersionData();


        var release = new TrackRelease
        {
            Name = versionData.AppVersion.ToString(),
            Status = configs.Value.ReleaseStatus,
            VersionCodes = [versionData.VersionCode],
        };

        var track = new Track
        {
            Releases = [release]
        };

        logger.LogDebug("Created track update data: {@Track}", track);

        var res = await service
            .Edits.Tracks
            .Update(
                track,
                configs.Value.ApplicationId,
                editId,
                configs.Value.ReleaseTrack
            )
            .ExecuteAsync(token);


        logger.LogDebug("Saved changes to track: {@Track}", res);
        return res;
    }
}
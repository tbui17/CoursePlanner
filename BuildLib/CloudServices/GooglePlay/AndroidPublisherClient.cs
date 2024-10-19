using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

[Inject]
public class AndroidPublisherClient(
    AndroidPublisherService service,
    IOptions<AppConfiguration> configs,
    ILogger<AndroidPublisherClient> logger,
    IServiceProvider provider
)
{
    private static AppEdit CreateEdit(ExpiryTime? time = default)
    {
        time ??= new();
        return new AppEdit
        {
        };
    }

    public Task<AppEdit> Probe()
    {
        var appEdit = CreateEdit(60);

        return service.Edits.Insert(appEdit, configs.Value.ApplicationId).ExecuteAsync();
    }

    public async Task<AppEdit> InsertEdit(ExpiryTime? time = default)
    {
        var appEdit = CreateEdit(time);
        var res = await service
            .Edits
            .Insert(appEdit, configs.Value.ApplicationId)
            .ExecuteAsync();
        logger.LogDebug("Created edit {Id} {ETag} {ExpiryTimeSeconds}", res.Id, res.ETag, res.ExpiryTimeSeconds);
        return res;
    }

    public async Task<BundlesListResponse> GetBundles()
    {
        var resp = await InsertEdit();
        return await service.Edits.Bundles.List(configs.Value.ApplicationId, resp.Id).ExecuteAsync();
    }

    public async Task<int> GetLatestVersionCode()
    {
        var resp = await GetBundles();
        return resp.Bundles.Max(x => x.VersionCode) ?? throw new InvalidDataException("No version code found");
    }

    public async Task<AppEdit> UploadBundleEdit(Stream stream, CancellationToken token)
    {
        var edit = await InsertEdit(9000);


        var req = service.Edits.Bundles.Upload(configs.Value.ApplicationId,
            edit.Id,
            stream,
            "application/octet-stream"
        );


        AddProgressLogger();

        logger.LogDebug("Starting upload for {Id}", edit.Id);
        var res = await req.UploadAsync(token);
        if (res.Exception is { } e)
        {
            throw e;
        }


        logger.LogDebug("Upload completed for {Id} {Exception} {BytesSent} {Status}",
            edit.Id,
            res.Exception,
            res.BytesSent,
            res.Status
        );

        return edit;

        void AddProgressLogger()
        {
            req.ProgressChanged += progress =>
            {
                logger.LogDebug("Progress details: {Status} {BytesSent} {@Exception}",
                    progress.Status,
                    progress.BytesSent,
                    progress.Exception
                );
            };
            req.ResponseReceived += bundle =>
            {
                logger.LogDebug("Bundle details: {VersionCode} {Etag} {Sha256} {Sha1}",
                    bundle.VersionCode,
                    bundle.ETag,
                    bundle.Sha256,
                    bundle.Sha1
                );
            };
            req.UploadSessionData += session => { logger.LogDebug("Session details: {UploadUri}", session.UploadUri); };
        }
    }

    public async Task UploadBundle(Stream stream, CancellationToken token)
    {
        var edit = await UploadBundleEdit(stream, token);
        // TODO: Refactor cyclic dependencies, break up classes
        var trackClient = provider.GetRequiredService<TrackClient>();
        await trackClient.UpdateTrack(edit.Id);

        logger.LogDebug("Committing edit for {Id}", edit.Id);
        var res2 = await service.Edits.Commit(configs.Value.ApplicationId, edit.Id).ExecuteAsync(token);
        logger.LogDebug("Edit committed for {Id} {@Response}", edit.Id, res2);
    }
}
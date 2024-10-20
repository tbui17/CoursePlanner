using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface IEditService
{
    Task<AppEdit> InsertEdit();
    Task<BundlesListResponse> GetBundles();
    Task<AppEdit> UploadBundle(Stream stream, CancellationToken token);
}

[Inject(typeof(IEditService), Lifetime = ServiceLifetime.Singleton)]
public class EditService(
    AndroidPublisherService service,
    IOptions<AppConfiguration> configs,
    ILogger<IEditService> logger) : IEditService
{
    public async Task<AppEdit> InsertEdit()
    {
        var res = await service
            .Edits
            .Insert(new AppEdit(), configs.Value.ApplicationId)
            .ExecuteAsync();

        return res;
    }

    public async Task<BundlesListResponse> GetBundles()
    {
        var resp = await InsertEdit();
        return await service.Edits.Bundles.List(configs.Value.ApplicationId, resp.Id).ExecuteAsync();
    }

    public async Task<AppEdit> UploadBundle(Stream stream, CancellationToken token)
    {
        var edit = await InsertEdit();


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
}
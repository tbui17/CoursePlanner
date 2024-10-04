using BuildLib.Secrets;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.Clients;

public class AndroidPublisherClient(
    BaseClientService.Initializer initializer,
    IOptions<CoursePlannerSecrets> secrets,
    ILogger<AndroidPublisherClient> logger)
{
    private readonly AndroidPublisherService _service = new(initializer);

    private static AppEdit CreateEdit(ExpiryTime? time = default)
    {
        time ??= new();
        return new AppEdit
        {
            ExpiryTimeSeconds = time,
        };
    }

    public Task<AppEdit> Probe()
    {
        var appEdit = CreateEdit(60);

        return _service.Edits.Insert(appEdit, secrets.Value.ApplicationId).ExecuteAsync();
    }

    private async Task<AppEdit> InsertEdit(ExpiryTime? time = default)
    {
        var appEdit = CreateEdit(time);
        var res = await _service
            .Edits
            .Insert(appEdit, secrets.Value.ApplicationId)
            .ExecuteAsync();
        logger.LogInformation("Created edit {Id} {ETag} {ExpiryTimeSeconds}", res.Id, res.ETag, res.ExpiryTimeSeconds);
        return res;
    }

    public async Task<BundlesListResponse> GetBundles()
    {
        var resp = await InsertEdit();
        return await _service.Edits.Bundles.List(secrets.Value.ApplicationId, resp.Id).ExecuteAsync();
    }

    public async Task UploadBundle(Stream stream, CancellationToken token)
    {
        var resp = await InsertEdit();

        var req = _service.Edits.Bundles.Upload(secrets.Value.ApplicationId,
            resp.Id,
            stream,
            "application/octet-stream"
        );

        AddProgressLogger();

        logger.LogInformation("Starting upload for {Id}", resp.Id);
        var res = await req.UploadAsync(token);
        if (res.Exception is { } e)
        {
            throw e;
        }

        logger.LogInformation("Upload completed for {Id} {Exception} {BytesSent} {Status}",
            resp.Id,
            res.Exception,
            res.BytesSent,
            res.Status
        );

        logger.LogInformation("Committing edit for {Id}", resp.Id);
        var res2 = await _service.Edits.Commit(secrets.Value.ApplicationId, resp.Id).ExecuteAsync(token);
        logger.LogInformation("Edit committed for {Id} {@Response}", resp.Id, res2);

        return;

        void AddProgressLogger()
        {
            req.ProgressChanged += progress =>
            {
                logger.LogInformation("Progress details: {Status} {BytesSent} {@Exception}",
                    progress.Status,
                    progress.BytesSent,
                    progress.Exception
                );
            };
            req.ResponseReceived += bundle =>
            {
                logger.LogInformation("Bundle details: {VersionCode} {Etag} {Sha256} {Sha1}",
                    bundle.VersionCode,
                    bundle.ETag,
                    bundle.Sha256,
                    bundle.Sha1
                );
            };
            req.UploadSessionData += session =>
            {
                logger.LogInformation("Session details: {UploadUri}", session.UploadUri);
            };
        }
    }
}

public class ExpiryTime(int seconds = 3600)
{
    public int GetValue()
    {
        if (seconds <= 0)
        {
            throw new ArgumentException("Expiry time must be greater than 0");
        }

        return seconds;
    }

    public static implicit operator string(ExpiryTime expiryTime) => expiryTime.GetValue().ToString();
    public static implicit operator ExpiryTime(int seconds) => new(seconds);
}
using BuildLib.Secrets;
using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface IBundleService
{
    Task<BundlesListResponse> GetBundles();
    Task UploadBundle(Stream stream, CancellationToken token);
}

[Inject(typeof(IBundleService), Lifetime = ServiceLifetime.Singleton)]
public class BundleService(
    AndroidPublisherService service,
    IOptions<IGooglePlayDeveloperApiConfigurationProxy> configs,
    ILogger<BundleService> logger,
    IVersionService versionService,
    IBundleProvider bundleProvider,
    IEditProvider editProvider
) : IBundleService
{
    private const string ContentType = "application/octet-stream";

    public async Task<BundlesListResponse> GetBundles()
    {
        return await bundleProvider.Get();
    }


    public async Task UploadBundle(Stream stream, CancellationToken token)
    {
        var edit = editProvider.EditId;
        if (await GetDuplicateBundle() is { } b)
        {
            logger.LogInformation("Bundle already exists {@Bundle}, skipping upload", b);
            return;
        }


        var req = service.Edits.Bundles.Upload(configs.Value.PackageName,
            edit,
            stream,
            ContentType
        );


        AddProgressLogger();

        logger.LogDebug("Starting upload for {Id}", edit);
        var res = await req.UploadAsync(token);
        if (res.Exception is { } e)
        {
            throw e;
        }


        logger.LogDebug("Upload completed for {Id} {Exception} {BytesSent} {Status}",
            edit,
            res.Exception,
            res.BytesSent,
            res.Status
        );

        return;

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

    private async Task<Bundle?> GetDuplicateBundle()
    {
        var bundles = await GetBundles();
        var projectVersionData = await versionService.GetProjectVersionData();
        return bundles.Bundles.SingleOrDefault(x => x.VersionCode is { } v && v == projectVersionData.VersionCode);
    }
}
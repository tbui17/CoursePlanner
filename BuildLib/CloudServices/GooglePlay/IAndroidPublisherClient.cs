using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface IAndroidPublisherClient
{
    Task<string> Probe();
    Task<BundlesListResponse> GetBundles();
    Task UploadBundle(Stream stream, CancellationToken token);
}

[Inject(typeof(IAndroidPublisherClient), Lifetime = ServiceLifetime.Singleton)]
public class AndroidPublisherClient(
    AndroidPublisherService service,
    IOptions<AppConfiguration> configs,
    ILogger<IAndroidPublisherClient> logger,
    IEditProvider editProvider,
    IBundleService bundleService,
    ITrackService trackService
) : IAndroidPublisherClient
{
    public Task<string> Probe()
    {
        return Task.FromResult(editProvider.EditId);
    }

    public async Task<BundlesListResponse> GetBundles()
    {
        return await bundleService.GetBundles();
    }

    public async Task UploadBundle(Stream stream, CancellationToken token)
    {
        var edit = editProvider.EditId;
        await bundleService.UploadBundle(stream, token);

        await trackService.UpdateTrack(edit, token);

        logger.LogDebug("Committing edit for {Id}", edit);
        var res = await service
            .Edits.Commit(configs.Value.ApplicationId, edit)
            .ExecuteAsync(token);
        logger.LogDebug("Edit committed for {Id} {@Response}", edit, res);
    }
}
using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface IAndroidPublisherClient
{
    Task<AppEdit> Probe();
    Task<BundlesListResponse> GetBundles();
    Task<int> GetLatestVersionCode();
    Task UploadBundle(Stream stream, CancellationToken token);
}

[Inject(typeof(IAndroidPublisherClient))]
public class AndroidPublisherClient(
    AndroidPublisherService service,
    IOptions<AppConfiguration> configs,
    ILogger<IAndroidPublisherClient> logger,
    IEditService editService,
    ITrackService trackService
) : IAndroidPublisherClient
{
    public Task<AppEdit> Probe() => editService.InsertEdit();

    public async Task<BundlesListResponse> GetBundles()
    {
        var resp = await editService.InsertEdit();
        return await service.Edits.Bundles.List(configs.Value.ApplicationId, resp.Id).ExecuteAsync();
    }

    public async Task<int> GetLatestVersionCode()
    {
        var resp = await GetBundles();
        return resp.Bundles.Max(x => x.VersionCode) ?? throw new InvalidDataException("No version code found");
    }

    public async Task UploadBundle(Stream stream, CancellationToken token)
    {
        var edit = await editService.UploadBundle(stream, token);
        await trackService.UpdateTrack(edit.Id, token);

        logger.LogDebug("Committing edit for {Id}", edit.Id);
        var res = await service
            .Edits.Commit(configs.Value.ApplicationId, edit.Id)
            .ExecuteAsync(token);
        logger.LogDebug("Edit committed for {Id} {@Response}", edit.Id, res);
    }
}
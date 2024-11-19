using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface IBundleProvider
{
    Task<BundlesListResponse> Get(CancellationToken token = default);
}

[Inject(typeof(IBundleProvider), Lifetime = ServiceLifetime.Singleton)]
public class BundleProvider(
    IEditProvider provider,
    AndroidPublisherService service,
    IOptions<IGooglePlayDeveloperApiConfigurationProxy> configs,
    ILogger<BundleProvider> logger
)
    : IBundleProvider
{
    public async Task<BundlesListResponse> Get(CancellationToken token = default)
    {
        var res = await service.Edits.Bundles.List(configs.Value.PackageName, provider.EditId).ExecuteAsync(token);
        logger.LogDebug("Received bundles {@Response}", res);
        return res;
    }
}
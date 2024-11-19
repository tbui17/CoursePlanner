using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices.GooglePlay;

public interface IEditProvider
{
    public string EditId { get; }
    public Task SetNewEditAsync();
}

[Inject(typeof(IEditProvider), Lifetime = ServiceLifetime.Singleton)]
public class EditService(
    AndroidPublisherService service,
    IOptions<IGooglePlayDeveloperApiConfigurationProxy> configs,
    ILogger<EditService> logger) : IEditProvider
{
    private AppEdit? _edit;
    private AppEdit Edit => _edit ?? CreateNewEditOrThrowAsync().Result;
    public string EditId => Edit.Id;

    public async Task SetNewEditAsync()
    {
        await CreateNewEditAsync();
    }

    private async Task<AppEdit> CreateNewEditAsync()
    {
        var res = await service
            .Edits
            .Insert(new AppEdit(), configs.Value.PackageName)
            .ExecuteAsync()
            .ConfigureAwait(false);

        logger.LogDebug("Inserted edit: {@Edit}", res);

        _edit = res;

        return res;
    }

    private async Task<AppEdit> CreateNewEditOrThrowAsync()
    {
        if (_edit is not null)
        {
            throw new InvalidOperationException("Edit already initialized");
        }

        return await CreateNewEditAsync();
    }
}
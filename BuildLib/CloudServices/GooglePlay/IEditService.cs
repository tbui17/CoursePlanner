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
}

[Inject(typeof(IEditProvider), Lifetime = ServiceLifetime.Singleton)]
public class EditService(
    AndroidPublisherService service,
    IOptions<GooglePlayDeveloperApiConfiguration> configs,
    ILogger<EditService> logger) : IEditProvider
{
    private AppEdit? _edit;
    private AppEdit Edit => _edit ?? InsertEdit().Result;
    public string EditId => Edit.Id;


    private async Task<AppEdit> InsertEdit()
    {
        if (_edit is not null)
        {
            throw new InvalidOperationException("Edit already initialized");
        }

        var res = await service
            .Edits
            .Insert(new AppEdit(), configs.Value.ProjectName)
            .ExecuteAsync()
            .ConfigureAwait(false);

        logger.LogDebug("Inserted edit: {@Edit}", res);

        _edit = res;

        return res;
    }
}
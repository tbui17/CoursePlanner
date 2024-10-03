using BuildLib.Secrets;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace BuildLib.Clients;

public class AndroidPublisherClient(BaseClientService.Initializer initializer, IOptions<CoursePlannerSecrets> secrets)
{
    public EditsResource.InsertRequest ProbeInsertRequest()
    {
        var service = new AndroidPublisherService(initializer);
        var appEdit = new AppEdit { Id = Guid.NewGuid().ToString(), ExpiryTimeSeconds = "60" };
        return service.Edits.Insert(appEdit, secrets.Value.ApplicationId);
    }
}
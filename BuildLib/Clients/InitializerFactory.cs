using BuildLib.Secrets;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace BuildLib.Clients;

public class InitializerFactory(IOptions<CoursePlannerSecrets> secrets)
{
    public BaseClientService.Initializer Create()
    {
        var serviceCredInitializer =
            new ServiceAccountCredential.Initializer(secrets.Value.GoogleServiceAccount.ClientEmail)
            {
                Scopes = [AndroidPublisherService.Scope.Androidpublisher]
            }.FromPrivateKey(secrets.Value.GoogleServiceAccount.PrivateKey);
        var serviceCred = new ServiceAccountCredential(serviceCredInitializer);

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = serviceCred,
        };

        return initializer;
    }
}
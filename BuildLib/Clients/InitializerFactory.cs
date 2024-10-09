using BuildLib.Secrets;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace BuildLib.Clients;

public class InitializerFactory(IOptions<GoogleServiceAccount> configs)
{
    public BaseClientService.Initializer Create()
    {
        var secrets = configs.Value;

        var serviceCredInitializer =
            new ServiceAccountCredential.Initializer(secrets.ClientEmail)
            {
                Scopes = [AndroidPublisherService.Scope.Androidpublisher]
            }.FromPrivateKey(secrets.PrivateKey);
        var serviceCred = new ServiceAccountCredential(serviceCredInitializer);

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = serviceCred,
        };

        return initializer;
    }
}
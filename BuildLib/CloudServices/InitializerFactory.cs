using BuildLib.Secrets;
using BuildLib.Utils;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace BuildLib.CloudServices;

[Inject]
public class InitializerFactory(IOptions<AppConfiguration> configs)
{
    public BaseClientService.Initializer Create()
    {
        var secrets = configs.Value.GoogleServiceAccount;

        var serviceCredInitializer =
            new ServiceAccountCredential.Initializer(secrets.ClientEmail)
            {
                Scopes = [AndroidPublisherService.Scope.Androidpublisher],
            }.FromPrivateKey(secrets.PrivateKey);
        var serviceCred = new ServiceAccountCredential(serviceCredInitializer);

        var initializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = serviceCred,
            ApplicationName = configs.Value.ApplicationId
        };

        return initializer;
    }
}
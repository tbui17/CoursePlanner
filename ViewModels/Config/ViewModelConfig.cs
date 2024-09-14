using Lib.Config;
using Lib.Utils;
using Plugin.LocalNotification;
using ViewModels.Services;


namespace ViewModels.Config;

public class ViewModelConfig(IServiceCollection services)
{
    public IServiceCollection AddServices()
    {
        services
            .AddBackendServices()
            .AddSingleton(LocalNotificationServiceFactory)
            .AddSingleton<ILocalNotificationService, LocalNotificationService>(x =>
                {
                    var instance = x.CreateInstance<LocalNotificationService>();
                    instance.StartListening();
                    return instance;
                }
            );
        return services;

        INotificationService? LocalNotificationServiceFactory() => LocalNotificationCenter.Current;
    }
}
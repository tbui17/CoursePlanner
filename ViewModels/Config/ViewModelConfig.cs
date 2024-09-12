using Lib.Config;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Plugin.LocalNotification;
using ViewModels.Services;
using ViewModels.Services.NotificationDataStreamFactory;

// ReSharper disable RedundantNameQualifier


namespace ViewModels.Config;

public class ViewModelConfig(AssemblyService assemblyService, IServiceCollection services)
{
    public ViewModelConfig(IServiceCollection services) : this(new AssemblyService(AppDomain.CurrentDomain), services)
    {
    }

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
            })
            .AddSingleton<NotificationTypes>(_ => GetNotificationTypes());
        return services;

        INotificationService? LocalNotificationServiceFactory() => LocalNotificationCenter.Current;
    }

    // TODO: Move to service
    private NotificationTypes GetNotificationTypes()
    {
        return assemblyService
            .GetConcreteClassesInNamespace(NamespaceData.FromNameofExpression(nameof(ViewModels.Domain)))
            .Where(x => x.IsAssignableTo(typeof(INotification)))
            .Where(x => x != typeof(Assessment))
            .Select(x => x.Name)
            .Concat(["Objective Assessment", "Performance Assessment"])
            .ToList()
            .Thru(x => new NotificationTypes(x));
    }
}

public record NotificationTypes(List<string> Value);
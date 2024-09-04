using Lib.Attributes;
using Lib.ExceptionHandlers;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Plugin.LocalNotification;
using ViewModels.Domain;
using ViewModels.ExceptionHandlers;
using ViewModels.Services;
using ViewModels.Setup;

// ReSharper disable RedundantNameQualifier


namespace ViewModels.Config;

public class ViewModelConfig(AssemblyService assemblyService, IServiceCollection services)
{
    public IServiceCollection AddServices()
    {
        services
            .AddSingleton(LocalNotificationServiceFactory)
            .AddSingleton<ILocalNotificationService, LocalNotificationService>(x =>
            {
                var instance = x.CreateInstance<LocalNotificationService>();
                instance.StartListening();
                return instance;
            })
            .AddSingleton<NotificationTypes>(_ => GetNotificationTypes())
            .AddTransient<GlobalExceptionHandler>()
            .AddSingleton<ClientExceptionHandler>()
            .AddSingleton<ISessionService, SessionService>()
            .AddTransient<DbSetup>()
            .AddInjectables(AppDomain.CurrentDomain)
            .AddTransient<SetupClient>()
            .AddSingleton<AppShellViewModel>();
        return services;

        INotificationService? LocalNotificationServiceFactory() => LocalNotificationCenter.Current;
    }

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

public record NotificationTypes(IList<string> Value);
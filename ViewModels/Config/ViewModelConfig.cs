using CommunityToolkit.Mvvm.ComponentModel;
using Lib.ExceptionHandlers;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Plugin.LocalNotification;
using ReactiveUI;
using ViewModels.Domain;
using ViewModels.ExceptionHandlers;
using ViewModels.Services;
using ViewModels.Setup;

// ReSharper disable RedundantNameQualifier


namespace ViewModels.Config;

public class ViewModelConfig(AssemblyService assemblyService, IServiceCollection services)
{
    private IServiceCollection AddViewModels()
    {
        var types = new[] { typeof(ObservableObject), typeof(ReactiveObject) };
        foreach (var vmType in assemblyService
                     .GetConcreteClassesInNamespace(NamespaceData.FromNameofExpression(nameof(ViewModels.Domain)))
                     .Where(x => types.Any(x.IsAssignableTo))
                )
        {
            services.AddTransient(vmType);
        }

        return services
            .AddSingleton<AppShellViewModel>();
    }

    public IServiceCollection AddServices()
    {
        AddViewModels()
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
            .AddTransient<SetupClient>();
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

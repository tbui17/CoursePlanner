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

public static class ViewModelConfig
{


    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        var types = new[]{typeof (ObservableObject) , typeof(ReactiveObject)};
        foreach (var vmType in AppDomain.CurrentDomain
                     .GetConcreteClassesInSameNameSpace<MainViewModel>()
                     .Where(x => types.Any(x.IsAssignableTo))
                )
        {
            services.AddTransient(vmType);
        }

        return services
            .AddSingleton<AppShellViewModel>();
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddViewModels()
            .AddSingleton(LocalNotificationServiceFactory)
            .AddSingleton<ILocalNotificationService, LocalNotificationService>(x =>
            {
                var instance = x.CreateInstance<LocalNotificationService>();
                instance.StartListening();
                return instance;
            })
            .AddKeyedSingleton<IList<string>>(nameof(TypesSource), (_, _) => TypesSource.Get())
            .AddTransient<GlobalExceptionHandler>()
            .AddSingleton<ClientExceptionHandler>()
            .AddSingleton<ISessionService, SessionService>()
            .AddTransient<DbSetup>()
            .AddTransient<SetupClient>();
        return services;

        INotificationService? LocalNotificationServiceFactory() => LocalNotificationCenter.Current;
    }

    public static IServiceCollection AddAssemblyNames(this IServiceCollection services, ICollection<string> names)
    {
        return services.AddKeyedSingleton(nameof(AddAssemblyNames), names);
    }
}

internal static class TypesSource
{
    public static List<string> Get()
    {
        return AppDomain.CurrentDomain
            .GetConcreteClassesInSameNameSpace<MainViewModel>()
            .Where(x => x.IsAssignableTo(typeof(INotification)))
            .Where(x => x != typeof(Assessment))
            .Where(x => x.IsClass)
            .Select(x => x.Name)
            .Concat(["Objective Assessment", "Performance Assessment"])
            .ToList();
    }
}
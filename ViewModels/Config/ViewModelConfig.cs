using Lib.ExceptionHandlers;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Plugin.LocalNotification;
using ViewModels.Domain;
using ViewModels.Events;
using ViewModels.ExceptionHandlers;
using ViewModels.Interfaces;
using ViewModels.Services;
using ViewModels.Setup;


namespace ViewModels.Config;

public static class ViewModelConfig
{
    public static IServiceCollection AddTransientRefreshable<T>(this IServiceCollection services)
        where T : class, IRefreshId =>
        services.AddTransient<T>(x =>
        {
            var subj = x.GetRequiredService<NavigationSubject>();
            var instance = x.CreateInstance<T>();
            subj.Subscribe(instance);
            return instance;
        });

    private static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        return services
            .AddTransient<MainViewModel>()
            .AddTransient<DetailedTermViewModel>()
            .AddTransient<EditTermViewModel>()
            .AddTransient<DetailedCourseViewModel>()
            .AddTransient<EditCourseViewModel>()
            .AddTransient<EditNoteViewModel>()
            .AddTransient<EditAssessmentViewModel>()
            .AddTransient<LoginViewModel>()
            .AddTransient<TermViewModel>()
            .AddTransient<NotificationDataViewModel>()
            .AddTransient<InstructorFormViewModelFactory>()
            .AddTransient<StatsViewModel>()
            .AddSingleton<AppShellViewModel>();
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddViewModels()
            .AddSingleton<NavigationSubject>()
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
            .AddTransient<SetupClient>()
            .AddTransient<RefreshableViewService>(provider =>
            {
                var x = provider.CreateInstance<RefreshableViewService>();
                x.RefreshableViewType = typeof(IRefreshableView<IRefreshId>);
                x.AssemblyNames = provider.GetRequiredKeyedService<ICollection<string>>(nameof(AddAssemblyNames));
                return x;
            });
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
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.Namespace == Util.NameOf(nameof(Lib.Models)))
            .Where(x => x.IsAssignableTo(typeof(INotification)))
            .Where(x => x != typeof(Assessment))
            .Where(x => x.IsClass)
            .Select(x => x.Name)
            .Concat(["Objective Assessment", "Performance Assessment"])
            .ToList();
    }
}
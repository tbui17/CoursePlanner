using Lib.Services;
using Lib.Utils;
using Plugin.LocalNotification;
using ViewModels.Domain;
using ViewModels.Events;
using ViewModels.Interfaces;
using ViewModels.Domain;
using ViewModels.Services;
using AppShellViewModel = ViewModels.Domain.AppShellViewModel;
using DetailedCourseViewModel = ViewModels.Domain.DetailedCourseViewModel;
using DetailedTermViewModel = ViewModels.Domain.DetailedTermViewModel;
using EditAssessmentViewModel = ViewModels.Domain.EditAssessmentViewModel;
using EditCourseViewModel = ViewModels.Domain.EditCourseViewModel;
using EditNoteViewModel = ViewModels.Domain.EditNoteViewModel;
using EditTermViewModel = ViewModels.Domain.EditTermViewModel;
using LoginViewModel = ViewModels.Domain.LoginViewModel;
using TermViewModel = ViewModels.Domain.TermViewModel;

namespace ViewModels.Config;

public static class ViewModelConfig
{
    public static IServiceCollection AddTransientRefreshable<T>(this IServiceCollection services)
        where T : class, IRefresh =>
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
            .AddTransientRefreshable<DetailedTermViewModel>()
            .AddTransientRefreshable<EditTermViewModel>()
            .AddTransientRefreshable<DetailedCourseViewModel>()
            .AddTransientRefreshable<EditCourseViewModel>()
            .AddTransientRefreshable<EditNoteViewModel>()
            .AddTransientRefreshable<EditAssessmentViewModel>()
            .AddTransientRefreshable<LoginViewModel>()
            .AddTransientRefreshable<TermViewModel>()
            .AddTransient<NotificationDataViewModel>()
            .AddTransient<InstructorFormViewModelFactory>()
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
            .AddSingleton<ISessionService, SessionService>()
            .AddTransient<RefreshableViewService>(provider =>
            {
                var x = provider.CreateInstance<RefreshableViewService>();
                x.RefreshableViewType = typeof(IRefreshableView<IRefresh>);
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
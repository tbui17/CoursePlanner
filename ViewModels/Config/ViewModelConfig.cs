using Lib.ExceptionHandlers;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ViewModels.Domain;
using ViewModels.Events;
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

public delegate Page? MainPageGetter();

public class ClientExceptionHandler(
    ILogger<ClientExceptionHandler> logger,
    [FromKeyedServices(nameof(MainPageGetter))]
    MainPageGetter mainPageGetter,
    GlobalExceptionHandler globalExceptionHandler
)

{
    async void OnMauiExceptionsOnUnhandledException(object _, UnhandledExceptionEventArgs args)
    {
        if (mainPageGetter() is not { } p)
        {
            logger.LogCritical("Exception:\n{Exception}", args.ExceptionObject.ToString());
            logger.LogCritical("Unexpected null MainPage");
            throw new ApplicationException(args.ExceptionObject.ToString());
        }

        if (args.ExceptionObject is not Exception exception)
        {
            logger.LogCritical("Unexpected exception object type: {Type}, {Args}",
                args.ExceptionObject.GetType().Name, args.ToString());
            await p.DisplayAlert("Unexpected Application Error",
                "There was an error during exception handling. Please restart the application.", "Cancel");
            return;
        }

        try
        {
            var res = await globalExceptionHandler.HandleAsync(exception);
            if (res.UserFriendly)
            {
                await ShowInfo(res.Message);
                return;
            }

            await ShowUnexpectedError(
                "An unexpected error occurred during the application's lifecycle. Please restart the application to ensure data integrity. An error log can be found within the application's folder."
            );
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Unexpected error occurred during exception handling.");
        }

        return;

        async Task ShowUnexpectedError(string message)
        {
            logger.LogError(exception, "Unhandled exception:\n{Exception}", exception.ToString());

            await p.DisplayAlert("Unexpected Application Error", message, "Cancel");
        }

        async Task ShowInfo(string message)
        {
            logger.LogInformation("Domain exception: {Message}", message);
            await p.DisplayAlert("Info", message, "Cancel");
        }
    }
}
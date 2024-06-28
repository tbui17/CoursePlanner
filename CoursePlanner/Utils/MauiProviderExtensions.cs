using System.ComponentModel;
using CommunityToolkit.Maui;

namespace CoursePlanner.Utils;

public static class MauiProviderExtensions
{
    public static IServiceCollection AddSingletonWithShellRoute<TView, TViewModel>(this IServiceCollection services)
        where TView : NavigableElement where TViewModel : class, INotifyPropertyChanged
    {
        return services.AddSingletonWithShellRoute<TView, TViewModel>(typeof(TView).Name);
    }
    
    public static IServiceCollection AddTransientWithShellRoute<TView, TViewModel>(this IServiceCollection services)
        where TView : NavigableElement where TViewModel : class, INotifyPropertyChanged
    {
        return services.AddTransientWithShellRoute<TView, TViewModel>(typeof(TView).Name);
    }
    
    public static IServiceCollection AddScopedWithShellRoute<TView, TViewModel>(this IServiceCollection services)
        where TView : NavigableElement where TViewModel : class, INotifyPropertyChanged
    {
        return services.AddScopedWithShellRoute<TView, TViewModel>(typeof(TView).Name);
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Attributes;
using ViewModels.Events;

namespace ViewModels.Domain;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public partial class AppShellViewModel : ObservableObject
{
    public AppShellViewModel()
    {
        #if DEBUG
        _isDebug = true;
        #endif

        this.Subscribe<LoginEvent>(e => IsLoggedIn = e.Value is not null);
    }

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _isDebug;
}
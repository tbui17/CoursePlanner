using CommunityToolkit.Mvvm.ComponentModel;
using ViewModels.Events;

namespace ViewModels.PageViewModels;

public partial class AppShellViewModel : ObservableObject
{
    public AppShellViewModel()
    {
        this.Subscribe<LoginEvent>(e => IsLoggedIn = e.Value is not null);
    }

    [ObservableProperty]
    private bool _isLoggedIn;
}
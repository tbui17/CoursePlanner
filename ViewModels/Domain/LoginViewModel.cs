using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
public partial class LoginViewModel(
    IAppService appService,
    INavigationService navService,
    ISessionService sessionService) : ObservableObject, ILogin, IRefreshId
{
    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _username = "";

    public async Task RefreshAsync()
    {
        Username = "";
        Password = "";
        await sessionService.LogoutAsync();
        await Task.CompletedTask;
    }

    public Task Init(int _) => RefreshAsync();


    [RelayCommand]
    public async Task LoginAsync()
    {
        await sessionService
            .LoginAsync(new LoginDetails(this))
            .MatchAsync(
                async _ => await navService.GoToMainPageAsync(),
                async e => await appService.ShowErrorAsync(e.Message)
            );
    }

    [RelayCommand]
    public async Task DeleteAsync()
    {
        throw new NotImplementedException();
    }


    [RelayCommand]
    public async Task RegisterAsync()
    {
        await sessionService
            .RegisterAsync(new LoginDetails(this))
            .MatchAsync(
                _ => navService.GoToMainPageAsync(),
                e => appService.ShowErrorAsync(e.Message)
            );
    }
}
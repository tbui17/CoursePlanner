using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Utils;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace ViewModels.Domain;

public partial class LoginViewModel(
    IAppService appService,
    INavigationService navService,
    ISessionService sessionService) : ObservableObject, ILogin, IRefresh
{
    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";


    [RelayCommand]
    public async Task LoginAsync()
    {
        var res = await sessionService.LoginAsync(new LoginDetails(this));

        if (res.IsFailed)
        {
            await appService.ShowErrorAsync(res.ToErrorString());
            return;
        }

        await navService.GoToMainPageAsync();
    }


    [RelayCommand]
    public async Task RegisterAsync()
    {
        var res = await sessionService.RegisterAsync(new LoginDetails(this));
        if (res.IsFailed)
        {
            await appService.ShowErrorAsync(res.ToErrorString());
            return;
        }

        await navService.GoToMainPageAsync();
    }

    public async Task RefreshAsync()
    {
        Username = "";
        Password = "";
        await sessionService.LogoutAsync();
        await Task.CompletedTask;
    }

    public Task Init(int _) => RefreshAsync();
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ViewModels.Services;


namespace ViewModels.PageViewModels;

public partial class LoginViewModel(
    AccountService accountService,
    IAppService appService,
    INavigationService navService,
    ILogger<DetailedCourseViewModel> logger) : ObservableObject, ILogin
{
    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";


    [RelayCommand]
    public async Task LoginAsync()
    {
        var res = await accountService.LoginAsync(new LoginDetails(this));

        if (res.IsFailed)
        {
            await appService.ShowErrorAsync(res.ToErrorString());
        }

        await navService.GoToPageAsync(NavigationTarget.MainPage);
    }


    [RelayCommand]
    public async Task RegisterAsync()
    {
        var res = await accountService.CreateAsync(new LoginDetails(this));

        if (res.IsFailed)
        {
            await appService.ShowErrorAsync(res.ToErrorString());
        }

        await navService.GoToPageAsync(NavigationTarget.MainPage);
    }

}
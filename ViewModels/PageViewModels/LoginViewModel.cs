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
    ILogger<DetailedCourseViewModel> logger) : ObservableObject, ILogin, IRefresh
{
    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";


    [RelayCommand]
    public async Task LoginAsync()
    {
        logger.LogInformation("Login attempt: {Username}", Username);
        var res = await accountService.LoginAsync(new LoginDetails(this));

        if (res.IsFailed)
        {
            logger.LogInformation("Login failed: {Error}", res.ToErrorString());
            await appService.ShowErrorAsync(res.ToErrorString());
            return;
        }

        await navService.GoToAsync(NavigationTarget.MainPage);
    }


    [RelayCommand]
    public async Task RegisterAsync()
    {
        logger.LogInformation("Register attempt: {Username}", Username);
        var res = await accountService.CreateAsync(new LoginDetails(this));


        if (res.IsFailed)
        {
            logger.LogInformation("Register failed: {Error}", res.ToErrorString());
            await appService.ShowErrorAsync(res.ToErrorString());
            return;
        }

        logger.LogInformation("Register success: {Id} {Username}", res.Value.Id, Username);

        await navService.GoToAsync(NavigationTarget.MainPage);
    }

    public Task Refresh()
    {

        Username = "";
        Password = "";
        return Task.CompletedTask;
    }
}
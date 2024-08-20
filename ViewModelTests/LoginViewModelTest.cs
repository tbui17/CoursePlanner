﻿
using FluentAssertions;
using Lib.Models;
using Lib.Services;
using Microsoft.Extensions.Logging;
using Moq;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests;


public class LoginViewModelTest : BasePageViewModelTest
{


    private ILogin _loginInfo;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _loginInfo = new LoginDetails("Test", "Test12345678");
        Model = new LoginViewModel(
            appService: AppMock.Object,
            navService: NavMock.Object,
            accountService: Resolve<AccountService>(),
            logger: Resolve<ILogger<DetailedCourseViewModel>>()
        );

        var accountService = Resolve<AccountService>();
        var res = await accountService.CreateAsync(_loginInfo);

        res.IsFailed.Should().BeFalse();



    }
    private LoginViewModel Model { get; set; }



    [Test]
    public async Task RegisterAsync_Valid_CreatesDatabaseEntry()
    {
        Model.Username = _loginInfo.Username + "a";
        Model.Password = _loginInfo.Password + "a";
        await Model.RegisterAsync();

        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.GoToPageAsync(NavigationTarget.MainPage), Times.Once);
    }

    [Test]
    public async Task LoginAsync_Valid_NavigatesToMainPage()
    {

        Model.Username = _loginInfo.Username;
        Model.Password = _loginInfo.Password;
        await Model.LoginAsync();
        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.GoToPageAsync(NavigationTarget.MainPage), Times.Once);
    }
}
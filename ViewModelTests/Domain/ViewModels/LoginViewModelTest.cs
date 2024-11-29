using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Moq;
using ViewModels.Domain;
using ViewModels.Services;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels;

public class LoginViewModelTest : BasePageViewModelTest
{
    private AppShellViewModel _appShellViewModel;
    private ILogin _loginInfo;
    private ISessionService _sessionService;

    private LoginViewModel Model { get; set; }

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _sessionService = Resolve<ISessionService>();
        _appShellViewModel = Resolve<AppShellViewModel>();
        _loginInfo = new LoginDetails("Test", "Test12345678");
        Model = new LoginViewModel(
            appService: AppMock.Object,
            navService: NavMock.Object,
            sessionService: _sessionService
        );

        var accountService = Resolve<IAccountService>();
        var res = await accountService.CreateAsync(_loginInfo);

        res.IsError.Should().BeFalse();
    }


    [Test]
    public async Task RegisterAsync_Valid_CreatesDatabaseEntry()
    {
        Model.Username = _loginInfo.Username + "a";
        Model.Password = _loginInfo.Password + "a";
        await Model.RegisterAsync();

        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.GoToMainPageAsync(), Times.Once);
    }

    [Test]
    public async Task LoginAsync_Valid_NavigatesToTermListPage()
    {
        Model.Username = _loginInfo.Username;
        Model.Password = _loginInfo.Password;
        await Model.LoginAsync();
        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.GoToMainPageAsync(), Times.Once);
    }

    [Test]
    public async Task LoginAsync_Valid_PersistsSignInState()
    {
        _appShellViewModel.IsLoggedIn.Should().BeFalse();
        _sessionService.IsLoggedIn.Should().BeFalse();
        Model.Username = _loginInfo.Username;
        Model.Password = _loginInfo.Password;
        await Model.LoginAsync();
        AppMock.VerifyReceivedError(0);
        NavMock.Verify(x => x.GoToMainPageAsync(), Times.Once);
        _sessionService.IsLoggedIn.Should().BeTrue();
        _appShellViewModel.IsLoggedIn.Should().BeTrue();
    }


    [Test]
    public async Task LoginAsync_Invalid_ShowsError()
    {
        Model.Username = _loginInfo.Username;
        Model.Password = _loginInfo.Password + "a";

        await Model.LoginAsync();
        AppMock.VerifyReceivedError();
        NavMock.Verify(x => x.GoToMainPageAsync(), Times.Never);
    }


    [Test]
    public async Task RegisterAsync_Invalid_ShowsError()
    {
        Model.Username = 1000.Times(_ => _loginInfo.Username).StringJoin();
        Model.Password = "";
        await Model.RegisterAsync();
        AppMock.VerifyReceivedError();
        NavMock.Verify(x => x.GoToMainPageAsync(), Times.Never);
    }
}
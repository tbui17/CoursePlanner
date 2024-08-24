using FluentResults;
using Lib.Models;

namespace Lib.Services;

public interface ISessionService
{
    bool IsLoggedIn { get; }
    Task<Result<User>> Login(ILogin loginDetails);
    Task Logout();
}

public class SessionService(AccountService accountService) : ISessionService
{
    private User? _user;

    public bool IsLoggedIn => _user is not null;

    public async Task<Result<User>> Login(ILogin loginDetails)
    {
        var res = await accountService.LoginAsync(loginDetails);

        _user = res.IsSuccess
            ? res.Value
            : null;

        return res;
    }

    public async Task Logout()
    {
        _user = null;
        await Task.CompletedTask;
    }
}
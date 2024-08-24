using FluentResults;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ViewModels.Events;

namespace ViewModels.Services;

public interface ISessionService
{
    bool IsLoggedIn { get; }
    Task<Result<User>> LoginAsync(ILogin loginDetails);
    Task LogoutAsync();
    Task<Result<User>> RegisterAsync(ILogin loginDetails);
}

public class SessionService(AccountService accountService, ILogger<ISessionService> logger) : ISessionService
{
    private User? _user;

    private User? User
    {
        get => _user;
        set
        {
            new LoginEvent(value).Publish();
            _user = value;
        }
    }

    public bool IsLoggedIn => User is not null;

    public async Task<Result<User>> LoginAsync(ILogin loginDetails)
    {
        logger.LogInformation("Login attempt: {Username}", loginDetails.Username);
        var res = await accountService.LoginAsync(loginDetails);


        if (res.IsSuccess)
        {
            logger.LogInformation("Login success: {Username}", res.Value.Username);
            User = res.Value;
        }
        else
        {
            logger.LogInformation("Login failed: {Error}", res.ToErrorString());
            User = null;
        }

        return res;
    }

    public async Task<Result<User>> RegisterAsync(ILogin loginDetails)
    {
        logger.LogInformation("Register attempt: {Username}", loginDetails.Username);
        var res = await accountService.CreateAsync(loginDetails);

        if (res.IsFailed)
        {
            logger.LogInformation("Register failed: {Error}", res.ToErrorString());
            User = null;
        }
        else
        {
            logger.LogInformation("Register success: {Id} {Username}", res.Value.Id, res.Value.Username);
            User = res.Value;
        }


        return res;
    }


    public async Task LogoutAsync()
    {
        logger.LogInformation("Logout: {Username}", User?.Username);
        User = null;
        await Task.CompletedTask;
    }
}
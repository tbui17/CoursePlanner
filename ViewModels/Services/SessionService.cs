using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Services;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ViewModels.Events;

namespace ViewModels.Services;

public interface ISessionService
{
    bool IsLoggedIn { get; }
    IUserDetail? User { get; }
    IUserDetail GetOrThrowUser();
    Task<Result<IUserDetail>> LoginAsync(ILogin loginDetails);
    Task LogoutAsync();
    Task<Result<IUserDetail>> RegisterAsync(ILogin loginDetails);
    Task<Result<IUserSetting>> GetUserSettingsAsync();
}

[Inject(typeof(ISessionService), ServiceLifetime.Singleton)]
public class SessionService(IAccountService accountService, ILogger<ISessionService> logger) : ISessionService
{
    private IUserDetail? _user;

    public IUserDetail? User
    {
        get => _user;
        private set => _user = value;
    }

    public bool IsLoggedIn => User is not null;

    public IUserDetail GetOrThrowUser()
    {
        if (User is null)
        {
            throw new InvalidOperationException("User is not logged in");
        }

        return User;
    }

    public async Task<Result<IUserDetail>> LoginAsync(ILogin loginDetails)
    {
        logger.LogInformation("Login attempt: {Username}", loginDetails.Username);
        var res = await accountService.LoginAsync(loginDetails);

        res
            .IfError(e =>
                {
                    logger.LogInformation("Login failed: {Error}", e.Message);
                    User = null;
                }
            )
            .IfOk(u =>
                {
                    logger.LogInformation("Login success: {Username}", u.Username);
                    OnLogin(u);
                }
            );

        return res;
    }

    public async Task<Result<IUserDetail>> RegisterAsync(ILogin loginDetails)
    {
        logger.LogInformation("Register attempt: {Username}", loginDetails.Username);
        var res = await accountService.CreateAsync(loginDetails);


        res
            .IfError(e =>
                {
                    logger.LogInformation("Register failed: {Error}", e.Message);
                    User = null;
                }
            )
            .IfOk(u =>
                {
                    logger.LogInformation("Register success: {Id} {Username}", u.Id, u.Username);
                    OnLogin(u);
                }
            );


        return res;
    }

    public async Task<Result<IUserSetting>> GetUserSettingsAsync()
    {
        if (User is null)
        {
            return new DomainException("User is not logged in");
        }

        var res = await accountService.GetUserSettingsAsync(User.Id);
        return res.ToResult();
    }


    public async Task LogoutAsync()
    {
        logger.LogInformation("Logout: {Username}", User?.Username);
        User = null;
        await Task.CompletedTask;
    }

    private void OnLogin(IUserDetail user)
    {
        User = user;
        new LoginEvent(user).Publish();
    }
}
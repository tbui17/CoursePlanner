using System.Data;
using System.Security.Cryptography;
using FluentValidation;
using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;
using Lib.Utils;
using Lib.Validators;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public interface IAccountService
{
    Task<Result<IUserDetail>> LoginAsync(ILogin login);
    Task<Result<IUserDetail>> CreateAsync(ILogin login);
    Task<IUserSetting> GetUserSettingsAsync(int userId);
    Task UpdateUserSettingsAsync(IUserSettingForm settings);
}

[Inject(typeof(IAccountService))]
public class AccountService(
    ILocalDbCtxFactory factory,
    [FromKeyedServices(nameof(LoginFieldValidator))]
    IValidator<ILogin> fieldValidator,
    ILogger<AccountService> logger
) : IAccountService
{
    public async Task<Result<IUserDetail>> LoginAsync(ILogin login)
    {
        using var _ = logger.MethodScope();
        logger.LogInformation("Login attempt for {Username}", login.Username);
        return await fieldValidator
            .Check(login)
            .FlatMapAsync(async validatedLogin =>
                {
                    // reject if username doesn't exist
                    await using var db = await factory.CreateDbContextAsync();
                    logger.LogInformation("Login attempt for {Username}", login.Username);
                    var dbUser = await db
                        .Accounts
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x => x.Username == validatedLogin.Username);


                    if (dbUser is null)
                    {
                        logger.LogInformation("Failed to find user {Username}", login.Username);
                        return new DomainException($"The username {login.Username} does not exist");
                    }

                    // reject if password is incorrect
                    var pass = HashedLogin.Create(login, dbUser.Salt).Password;
                    if (pass != dbUser.Password)
                    {
                        logger.LogInformation("Failed login attempt for user {Username}", login.Username);
                        return new DomainException("Invalid password");
                    }

                    IUserDetail returnedUser = new User { Id = dbUser.Id, Username = dbUser.Username };
                    logger.LogInformation("Login success for {Id} {Username}", returnedUser.Id, returnedUser.Username);
                    return returnedUser.ToResult();
                }
            );
    }

    public async Task<Result<IUserDetail>> CreateAsync(ILogin login)
    {
        using var _ = logger.BeginScope("{Method}", nameof(CreateAsync));
        logger.LogInformation("Create attempt for {Username}", login.Username);
        // validate input
        if (fieldValidator.GetError(login) is { } exc)
        {
            logger.LogInformation("Validation failed for {Username}: {Error}", login.Username, exc.Message);
            return exc;
        }

        // create hash
        var hashedLogin = HashedLogin.Create(login);

        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

        // reject if username already exists
        var username = await db
            .Accounts.Where(x => x.Username == hashedLogin.Username)
            .Select(x => x.Username)
            .FirstOrDefaultAsync();

        if (username is not null)
        {
            logger.LogInformation("Failed to create user {Username}: username already exists", login.Username);
            return new DomainException($"Username {login.Username} already exists");
        }

        // save new user
        var account = new User
        {
            Username = hashedLogin.Username,
            Password = hashedLogin.Password,
            Salt = hashedLogin.Salt,
            UserSetting = UserSetting.DefaultUserSetting
        };
        logger.LogInformation("Creating user {Username} with settings {@Settings}",
            login.Username,
            account.UserSetting
        );

        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        var created = await db
            .Accounts.Where(x => x.Username == hashedLogin.Username)
            .Select(x => new User { Id = x.Id, Username = x.Username })
            .AsNoTracking()
            .SingleAsync();
        await tx.CommitAsync();
        logger.LogInformation("Created user {Id} {Username}", created.Id, created.Username);
        return created;
    }

    public async Task<IUserSetting> GetUserSettingsAsync(int userId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = CreateGetUserSettingsQuery(db, userId);

        var userSetting = await query.AsNoTracking().SingleAsync();
        return userSetting;
    }

    public async Task UpdateUserSettingsAsync(IUserSettingForm settings)
    {
        await using var db = await factory.CreateDbContextAsync();
        var query = CreateGetUserSettingsQuery(db, settings.UserId);

        var userSetting = await query.SingleAsync();
        userSetting.SetFromUserSetting(settings);
        await db.SaveChangesAsync();
    }

    private IQueryable<UserSetting> CreateGetUserSettingsQuery(LocalDbCtx db, int userId)
    {
        var query = db.UserSettings.Where(x => x.UserId == userId);

        return query;
    }
}

file record HashedLogin
{
    private HashedLogin()
    {
    }

    public string Username { get; private init; } = "";
    public string Password { get; private init; } = "";
    public byte[] Salt { get; private init; } = [];


    public static HashedLogin Create(ILogin login)
    {
        var salt = CreateSalt();
        return Create(login, salt);
    }


    public static HashedLogin Create(ILogin login, byte[] salt)
    {
        return new HashedLogin
        {
            Username = login.Username,
            Password = Hash(login.Password, salt),
            Salt = salt
        };
    }

    private static byte[] CreateSalt()
    {
        // Generate a 16 byte salt using a sequence of
        // cryptographically strong random bytes.
        return RandomNumberGenerator.GetBytes(16);
    }

    private static string Hash(string password, byte[] salt)
    {
        // https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing?view=aspnetcore-9.0

        // derive a 32 byte subkey (use HMACSHA512 with 100,000 iterations)
        var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 100000,
                numBytesRequested: 32
            )
        );

        return hashed;
    }
}
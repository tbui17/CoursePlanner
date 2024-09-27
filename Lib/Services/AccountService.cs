using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Lib.Attributes;
using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Lib.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.Services;

public interface IAccountService
{
    Task<Result<User>> LoginAsync(ILogin login);
    Task<Result<User>> CreateAsync(ILogin login);
    Task<Result<IUserSetting>> GetUserSettingsAsync(ILogin login);
}

[Inject(typeof(IAccountService))]
public class AccountService(
    ILocalDbCtxFactory factory,
    [FromKeyedServices(nameof(LoginFieldValidator))]
    IValidator<ILogin> fieldValidator,
    ILogger<AccountService> logger
) : IAccountService
{
    public async Task<Result<User>> LoginAsync(ILogin login)
    {
        using var _ = logger.MethodScope();
        logger.LogInformation("Login attempt for {Username}", login.Username);
        return await fieldValidator.Check(login)
            .Map(HashedLogin.Create)
            .FlatMapAsync(async hashedLogin =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var res = await db.Accounts
                    .Where(x => x.Username == hashedLogin.Username && x.Password == hashedLogin.Password)
                    .Select(x => new User { Id = x.Id, Username = x.Username })
                    .AsNoTracking()
                    .ToListAsync();

                logger.LogInformation("Login attempt for {Username} resulted in {Result}", login.Username, res);

                return res switch
                {
                    [var user] => user.ToResult(),
                    [] => new DomainException("Invalid username or password"),
                    var entries => throw new UnreachableException($"Unexpected duplicate entries: {entries}")
                    {
                        Data =
                        {
                            [nameof(entries)] = entries
                        }
                    }
                };
            });
    }

    public async Task<Result<User>> CreateAsync(ILogin login)
    {
        using var _ = logger.BeginScope("{Method}", nameof(CreateAsync));
        logger.LogInformation("Create attempt for {Username}", login.Username);
        if (fieldValidator.GetError(login) is {} exc)
        {
            return exc;
        }

        var hashedLogin = HashedLogin.Create(login);

        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

        var username = await db.Accounts.Where(x => x.Username == hashedLogin.Username)
            .Select(x => x.Username)
            .FirstOrDefaultAsync();

        if (username is not null)
        {
            return new DomainException($"Username {login.Username} already exists");
        }

        var account = new User
        {
            Username = hashedLogin.Username,
            Password = hashedLogin.Password
        };
        var setting = account.CreateUserSetting();
        db.Add(setting);

        await db.Accounts.AddAsync(account);
        await db.SaveChangesAsync();
        var created = await db.Accounts.Where(x => x.Username == hashedLogin.Username)
            .Select(x => new User { Id = x.Id, Username = x.Username })
            .AsNoTracking()
            .SingleAsync();
        await tx.CommitAsync();
        return created;
    }

    public Task<Result<IUserSetting>> GetUserSettingsAsync(ILogin login)
    {
        throw new NotImplementedException();
    }

    private record HashedLogin
    {
        private HashedLogin()
        {
        }

        public string Username { get; private init; } = "";
        public string Password { get; private init; } = "";


        public static HashedLogin Create(ILogin login) => new()
        {
            Username = login.Username,
            Password = Hash(login.Password)
        };

        private static string Hash(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA512.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    };
}
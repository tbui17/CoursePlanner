using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using FluentResults;
using FluentValidation;
using Lib.Models;
using Lib.Utils;
using Lib.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Services;

public class AccountService(
    ILocalDbCtxFactory factory,
    [FromKeyedServices(nameof(LoginFieldValidator))]
    IValidator<ILogin> fieldValidator
    )
{
    public async Task<Result<int>> LoginAsync(ILogin login)
    {
        return await fieldValidator.Check(login)
            .Map(HashedLogin.Create)
            .BindAsync(async hashedLogin =>
            {
                await using var db = await factory.CreateDbContextAsync();
                var res = await db.Accounts
                    .Where(x => x.Username == hashedLogin.Username && x.Password == hashedLogin.Password)
                    .Select(x => x.Id)
                    .ToListAsync();

                return res switch
                {
                    [var id] => id.ToResult(),
                    [] => Result.Fail("Invalid username or password"),
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
        if (fieldValidator.Check(login) is { IsFailed: true } exc)
        {
            return exc.ToResult();
        }
        var hashedLogin = HashedLogin.Create(login);

        await using var db = await factory.CreateDbContextAsync();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);

        var username = await db.Accounts.Where(x => x.Username == hashedLogin.Username)
            .Select(x => x.Username)
            .FirstOrDefaultAsync();

        if (username is not null)
        {
            return Result.Fail($"Username {login.Username} already exists");
        }

        var account = new User
        {
            Username = hashedLogin.Username,
            Password = hashedLogin.Password
        };

        await db.Accounts.AddAsync(account);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return account;
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
            Username = login.Username.ToLowerInvariant().Trim(),
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
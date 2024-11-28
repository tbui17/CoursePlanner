using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Testing;

namespace LibTests;

public class AccountServiceTest : BaseDbTest
{
    [Test]
    public async Task CreateAsync_ExcludesRelationsAndSensitiveFields()
    {
        var f = await CreateAccountFixture();
        var user = f.User;
        using var _ = new AssertionScope();
        user.Username.Should().Be(f.Login.Username);
        user.Password.Should().BeEmpty();
        user.Salt.Should().BeEmpty();
        user.UserSetting.Should().BeNull();
        user.Id.Should().BeGreaterThan(0);
    }

    private async Task<AccountFixture> CreateAccountFixture()
    {
        var accountService = new AccountService(
            factory: Resolve<IDbContextFactory<LocalDbCtx>>(),
            logger: new FakeLogger<AccountService>(),
            fieldValidator: Resolve<IValidator<ILogin>>()
        );
        var login = new LoginDetails("TestUser12345", "TestPass12345");

        var res = await accountService.CreateAsync(login);
        return new AccountFixture { AccountService = accountService, Login = login, Result = res };
    }


    [Test]
    public async Task AddNotificationRange_TwoMonths_PersistsValue()
    {
        var f = await CreateAccountFixture();
        var res = await f.AccountService.GetUserSettingsAsync(f.User.Id);

        var twoMonths = TimeSpan.FromDays(60);

        var setting = new UserSetting
        {
            UserId = f.User.Id,
            NotificationRange = twoMonths,
        };

        await f.AccountService.UpdateUserSettingsAsync(setting);
    }
}

internal record AccountFixture
{
    public required AccountService AccountService { get; set; }
    public required LoginDetails Login { get; set; }
    public required Result<User> Result { get; set; }
    public User User => Result.Unwrap();
}
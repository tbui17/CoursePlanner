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
    public async Task CreateAsync_ResultExcludesRelationsAndSensitiveFields()
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

    [Test]
    public async Task CreateAsync_CreatesRelatedEntities()
    {
        var f = await CreateAccountFixture();
        await f.AccountService.Awaiting(x => x.GetUserSettingsAsync(f.User.Id)).Should().NotThrowAsync();
    }

    [Test]
    public async Task CreateAsync_CreatesDefaultSettingValue()
    {
        var f = await CreateAccountFixture();
        var settings = await f.AccountService.GetUserSettingsAsync(f.User.Id);
        settings.Should().BeEquivalentTo(UserSetting.DefaultUserSettingValue);
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
    public async Task UpdateUserSettingsAsync_NotDefault_PersistsValue()
    {
        var f = await CreateAccountFixture();

        var twoMonths = TimeSpan.FromDays(60);

        var setting = new UserSetting
        {
            UserId = f.User.Id,
            NotificationRange = twoMonths,
        };

        await f.AccountService.UpdateUserSettingsAsync(setting);
        var res = await f.AccountService.GetUserSettingsAsync(f.User.Id);
        res.NotificationRange.Should().Be(twoMonths);
    }
}

internal record AccountFixture
{
    public required AccountService AccountService { get; set; }
    public required LoginDetails Login { get; set; }
    public required Result<User> Result { get; set; }
    public User User => Result.Unwrap();
}
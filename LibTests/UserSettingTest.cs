using System.Data;
using FluentAssertions;
using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace LibTests;


public class UserSettingTest : BaseDbTest
{

    [Test]
    public async Task AddNotificationRange_TwoMonths_PersistsValue()
    {
        await using var db = await GetDb();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var twoMonths = TimeSpan.FromDays(60);

        const int userId = 6172;

        var user = new User
        {
            Id = userId,
            Username = "TestUser12345",
            Password = "TestPassword12345",
        };

        db.Accounts.Add(user);
        await db.SaveChangesAsync();

        var setting = new UserSetting
        {
            UserId = userId,
            NotificationRange = twoMonths,
        };

        db.UserSettings.Add(setting);
        await db.SaveChangesAsync();
        var dbSetting = await db.UserSettings.SingleAsync(x => x.UserId == userId);
        dbSetting.NotificationRange.Should().Be(twoMonths);
        await tx.RollbackAsync();

    }

}
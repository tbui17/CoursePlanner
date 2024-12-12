using FluentAssertions;
using FluentAssertions.Extensions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;

namespace LibTests.NotificationTests;

public class NotificationUpcomingTest : BaseDbTest
{
    private INotificationDataService _notificationDataService;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _notificationDataService = Resolve<INotificationDataService>();
    }

    [Test]
    public async Task GetNotifications_NoEligible_IsEmpty()
    {
        await DisableNotifications();
        var userSetting = new UserSetting
        {
            NotificationRange = TimeSpan.FromDays(5)
        };

        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result.Should().BeEmpty();
    }

    private async Task DisableNotifications()
    {
        await using var db = await GetDb();
        var min = DateTime.MinValue.Date;

        var dbSets = db.GetDbSets<INotification>();

        foreach (var dbSet in dbSets)
        {
            await dbSet.ExecuteUpdateAsync(p => p
                .SetProperty(x => x.ShouldNotify, false)
                .SetProperty(x => x.Start, min)
                .SetProperty(x => x.End, min.AddDays(1))
            );
        }
    }


    [Test]
    public async Task GetNotifications_HasNotifications_ReturnsPolymorphicResult()
    {
        await DisableNotifications();
        await EnableNotifications(5.Days());
        var userSetting = new UserSetting
        {
            NotificationRange = TimeSpan.FromDays(99999)
        };
        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result
            .Select(x => x.Entity)
            .Should()
            .NotBeEmpty()
            .And.ContainItemsAssignableTo<Course>()
            .And.ContainItemsAssignableTo<Assessment>();
    }


    private async Task EnableNotifications(TimeSpan databaseStartTimesAheadBy, TimeSpan? databaseEndTimesAheadBy = null)
    {
        await using var db = await GetDb();
        var dbSets = db.GetDbSets<INotification>();
        var today = DateTime.Now.Date;
        // time span added to start date and end date
        // end date is always 1 day after start date
        var start = today.Add(databaseStartTimesAheadBy);

        var end = databaseEndTimesAheadBy is not null
            ? today.Add(databaseEndTimesAheadBy.Value)
            : start.AddDays(1);

        end.Should().BeOnOrAfter(start, "Invalid test setup");

        foreach (var dbSet in dbSets)
        {
            await dbSet
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.ShouldNotify, true)
                    .SetProperty(x => x.Start, start)
                    .SetProperty(x => x.End, end)
                );
        }
    }

    [Test]
    public async Task GetNotifications_5DaysInAdvance_ShouldBeInclusive()
    {
        await DisableNotifications();
        await EnableNotifications(0.Days());

        var userSetting = new UserSetting
        {
            NotificationRange = 5.Days()
        };

        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetNotifications_AllDbNotificationsGtNotificationRangeSetting_ReturnsNothing()
    {
        await DisableNotifications();
        await EnableNotifications(5.Days());

        var userSetting = new UserSetting
        {
            NotificationRange = 1.Days()
        };

        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetNotifications_AllDbNotificationsInThePast_ReturnsNothing()
    {
        await DisableNotifications();
        await EnableNotifications(-1.Days());

        var userSetting = new UserSetting
        {
            NotificationRange = 10.Days()
        };

        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result.Should().BeEmpty();
    }
}

public class TodayProvider20200110 : ITodayProvider
{
    public DateTime Today()
    {
        return new DateTime(2020, 1, 10);
    }
}
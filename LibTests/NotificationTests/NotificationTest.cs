using FluentAssertions;
using FluentAssertions.Extensions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests.NotificationTests;

public class NotificationUpcomingTest : BaseDbTest
{
    private NotificationDataService _notificationDataService;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _notificationDataService = Provider.GetRequiredService<NotificationDataService>();
    }

    [Test]
    public async Task GetNotifications_NoEligible_IsEmpty()
    {
        await DisableNotifications();
        var userSetting = new UserSetting
        {
            NotificationRange = TimeSpan.FromDays(5)
        };
        var notificationService = Provider.GetRequiredService<NotificationDataService>();
        var result = await notificationService.GetUpcomingNotifications(userSetting);
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
        var notificationService = Provider.GetRequiredService<NotificationDataService>();
        var result = await notificationService.GetUpcomingNotifications(userSetting);
        result
            .Select(x => x.Entity)
            .Should()
            .NotBeEmpty()
            .And.ContainItemsAssignableTo<Course>()
            .And.ContainItemsAssignableTo<Assessment>();
    }


    private async Task EnableNotifications(TimeSpan databaseStartTimesAheadBy, TimeSpan? databaseEndTimesAheadBy = null)
    {
        await DisableNotifications();
        await using var db = await GetDb();
        var dbSets = db.GetDbSets<INotification>();
        var today = DateTime.Now.Date;
        // time span added to start date and end date
        // end date is always 1 day after start date
        var start = today.Add(databaseStartTimesAheadBy);

        var end = databaseEndTimesAheadBy is not null
            ? today.Add(databaseEndTimesAheadBy.Value)
            : start.AddDays(1);

        end.Should().BeOnOrAfter(start,"Invalid test setup");

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
    public async Task GetNotifications_ShouldBeInclusive()
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
            NotificationRange = 10.Days()
        };

        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetNotifications_AllDbNotificationsInThePast_ReturnsNothing()
    {
        await DisableNotifications();
        await EnableNotifications(1.Days());

        var userSetting = new UserSetting
        {
            NotificationRange = 10.Days()
        };

        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);
        result.Should().NotBeEmpty();
    }
}
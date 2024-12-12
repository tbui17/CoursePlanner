using FluentAssertions;
using FluentAssertions.Extensions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.MultiDbContext;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibTests.NotificationTests;

public class NotificationUpcomingTest20200110 : NotificationUpcomingTest
{
    private INotificationDataService _notificationDataService;
    private TodayProvider20200110 _todayProvider;

    [SetUp]
    public override async Task Setup()
    {
        _todayProvider = new TodayProvider20200110();
        await base.Setup();
        _notificationDataService = new NotificationDataService(
            Resolve<MultiLocalDbContextFactory>(),
            Resolve<ILogger<INotificationDataService>>(),
            _todayProvider
        );
    }

    [Test]
    public async Task GetUpcomingNotifications_Jan10With10DaysInAdvance_GetsNotificationsByStartDateInclusiveJan20()
    {
        // Given:
        // The current date is 2020-01-10
        // There are only notifications with start dates from 2020-01-10 to 2020-01-20
        await DisableNotifications();
        await EnableNotifications(10.Days());

        // The user has a notification range of 10 days
        var userSetting = new UserSetting
        {
            NotificationRange = 10.Days()
        };


        // When upcoming notifications are retrieved
        var result = await _notificationDataService.GetUpcomingNotifications(userSetting);

        // Then there should be results
        result.Should().NotBeEmpty();
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

    private async Task EnableNotifications(TimeSpan databaseStartTimesAheadBy, TimeSpan? databaseEndTimesAheadBy = null)
    {
        await using var db = await GetDb();
        var dbSets = db.GetDbSets<INotification>();
        var today = _todayProvider.Today();

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
}
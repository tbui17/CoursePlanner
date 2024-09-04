using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests.NotificationTests;

public class NotificationUpcomingTest : BaseDbTest
{
    [Test]
    public async Task GetNotifications_NoEligible_IsEmpty()
    {
        await DisableNotifications();
        var userSetting = new UserSetting
        {
            NotificationRange = TimeSpan.FromDays(5)
        };
        var notificationService = Provider.GetRequiredService<NotificationService>();
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

        await using var db = await GetDb();
        var dbSets = db.GetDbSets<INotification>();
        var today = DateTime.Now.Date;
        var start = today.AddDays(5);
        var end = start.AddDays(1);

        foreach (var dbSet in dbSets)
        {
            await dbSet
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.ShouldNotify, true)
                    .SetProperty(x => x.Start, start)
                    .SetProperty(x => x.End, end)
                );
        }

        var userSetting = new UserSetting
        {
            NotificationRange = TimeSpan.FromDays(99999)
        };
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetUpcomingNotifications(userSetting);
        result
            .Select(x => x.Entity)
            .Should()
            .NotBeEmpty()
            .And.ContainItemsAssignableTo<Course>()
            .And.ContainItemsAssignableTo<Assessment>();
    }

    [TestCaseSource(nameof(NotificationTestCaseData))]
    public async Task GetNotifications(NotificationTestData data)
    {
        await DisableNotifications();
        await using var db = await GetDb();
        var dbSets = db.GetDbSets<INotification>();
        var today = DateTime.Now.Date;
        // time span added to start date and end date
        // end date is always 1 day after start date
        var start = today.Add(data.DatabaseTimesAheadBy);
        var end = start.AddDays(1);

        foreach (var dbSet in dbSets)
        {
            await dbSet
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.ShouldNotify, true)
                    .SetProperty(x => x.Start, start)
                    .SetProperty(x => x.End, end)
                );
        }

        var userSetting = new UserSetting
        {
            NotificationRange = data.NotificationRange
        };
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetUpcomingNotifications(userSetting);
        using var scope = new AssertionScope();
        data.Assertion?.Invoke(result);
        scope.BecauseOf(data.Description);
    }

    private static IEnumerable<TestCaseData> NotificationTestCaseData()
    {
        List<NotificationTestData> data =
        [
            new()
            {
                Description = "The notification range should be inclusive",
                DatabaseTimesAheadBy = 5.Days(),
                NotificationRange = 5.Days(),
                Assertion = x => x.Should().NotBeEmpty()
            },
            new()
            {
                DatabaseTimesAheadBy = -5.Days(),
                NotificationRange = 5.Days(),
                Assertion = x => x.Should().BeEmpty(),
            },
            new()
            {
                Description = "There should be no notifications when all notifications are 10 days ahead, but the setting only requests to check 5 days ahead.",
                DatabaseTimesAheadBy = 10.Days(),
                NotificationRange = 5.Days(),
                Assertion = x => x.Should().BeEmpty(),
            }
        ];


        return data.Select(x =>
        {
            var testCase = new TestCaseData(x);
            if (!string.IsNullOrWhiteSpace(x.Description))
            {
                testCase.SetName(x.Description);
            }

            return testCase;
        });
    }


    public record NotificationTestData(
        TimeSpan NotificationRange = default,
        TimeSpan DatabaseTimesAheadBy = default,
        Action<IList<INotificationDataResult>>? Assertion = default,
        string Description = ""

    );
}

class S : TestCaseData
{

}
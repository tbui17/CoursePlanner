using FluentAssertions;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests.NotificationTests;

public class NotificationItemsTest : BaseDbTest
{
    private NotificationService _notificationService;

    private DateTime _now;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        _notificationService = Provider.GetRequiredService<NotificationService>();
        _now = DateTime.Now.Date;
        var res = await _notificationService.GetNotificationsForMonth(_now);
        res.Should()
            .BeEmpty(
                $"Unexpected precondition: No notifications should be present for this month for this test fixture, but {res.Count} were found.");
    }

    [Test]
    public async Task GetNotificationsForMonth_ReturnsStartDatesWithinMonth()
    {
        await using var db = await GetDb();
        await db.Assessments.Take(2)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.Start, _now).SetProperty(x => x.ShouldNotify, true));

        var res = await _notificationService.GetNotificationsForMonth(_now);

        res.Should().HaveCount(2);
    }


    [Test]
    public async Task GetNotificationsForMonth_ReturnsStartAndEndDatesWithinMonth()
    {
        const int count = 2;
        await using var db = await GetDb();
        var assessments = await db.Assessments.Take(count).ToListAsync();
        var a1 = assessments[0];
        var a2 = assessments[1];

        a1.Start = _now;
        a1.End = _now.AddYears(1);
        a2.Start = _now.AddYears(1);
        a2.End = _now;
        a2.ShouldNotify = true;
        a1.ShouldNotify = true;

        await db.SaveChangesAsync();

        var res = await _notificationService.GetNotificationsForMonth(_now);

        res.Should().HaveCount(count);
    }
}
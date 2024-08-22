using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
using static Lib.Utils.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: NonParallelizable]

namespace LibTests;

public class NotificationUpcomingTest : BaseDbTest
{
    private IList<NotificationResult> Result { get; set; }


    [OneTimeSetUp]
    public override async Task Setup()
    {
        await base.Setup();
        var notificationSetupUtil = Provider.GetRequiredService<NotificationSetupUtil>();
        await notificationSetupUtil.SetStartTimes(DateTime.Now.Date);
        var notificationService = Provider.GetRequiredService<NotificationService>();
        Result = await notificationService.GetUpcomingNotifications();
    }

    [Test]
    public void GetNotifications_ReturnSome()
    {
        Result
            .Should()
            .HaveCount(2);
    }


    [Test]
    public void GetNotifications_ReturnAllTypes()
    {
        Result
            .Should()
            .HaveCount(2)
            .And
            .ContainSingle(x => x.Entity.Name == "Course 1")
            .And
            .ContainSingle(x => x.Entity.Name == "Assessment 1");
    }
}

public class NotificationLapsedTest : BaseDbTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        var notificationSetupUtil = Provider.GetRequiredService<NotificationSetupUtil>();
        await notificationSetupUtil.SetStartTimes(DateTime.Now.Date.AddSeconds(-1));
    }

    [Test]
    public async Task GetNotifications_ShouldNotNotify()
    {
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetUpcomingNotifications();
        result
            .Should()
            .HaveCount(0);
    }
}

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
        await db.Assessments.Take(2).ExecuteUpdateAsync(p => p.SetProperty(x => x.Start, _now));

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

        await db.SaveChangesAsync();

        var res = await _notificationService.GetNotificationsForMonth(_now);

        res.Should().HaveCount(count);
    }
}

public class NotificationSetupUtil(IDbContextFactory<LocalDbCtx> factory)
{
    public async Task SetStartTimes(DateTime time)
    {
        await using var db = await factory.CreateDbContextAsync();
        var course1 = await db
            .Courses
            .AsTracking()
            .FirstAsync(x => x.Id == 1);
        var assessment1 = await db
            .Assessments
            .AsTracking()
            .FirstAsync(x => x.Id == 1);

        course1.Start = time;
        assessment1.Start = time;
        course1.ShouldNotify = true;
        assessment1.ShouldNotify = true;
        await db.SaveChangesAsync();
    }
}
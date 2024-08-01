using FluentAssertions;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests;

public class NotificationUpcomingTest : BaseDbTest
{
    [SetUp]
    public new async Task Setup()
    {
        await NotificationSetupUtil.SetStartTimes(DateTime.Now.AddHours(23));
    }

    [Test]
    public async Task GetNotifications_ReturnSome()
    {
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetNotifications();
        result
           .Should()
           .HaveCount(2);
    }

    [Test]
    public async Task GetNotifications_ReturnAllTypes()
    {
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetNotifications();
        result
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
    public new async Task Setup()
    {
        await NotificationSetupUtil.SetStartTimes(DateTime.Now.AddHours(-1));
    }

    [Test]
    public async Task GetNotifications_ShouldNotNotify()
    {
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetNotifications();
        result
           .Should()
           .HaveCount(0);
    }
}

internal static class NotificationSetupUtil
{
    public static async Task SetStartTimes(DateTime time)
    {
        var db = new LocalDbCtx();

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
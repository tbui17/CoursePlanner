using FluentAssertions;
using Lib.Models;
using Lib.Services;
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
        await notificationSetupUtil.SetStartTimes(DateTime.Now.AddHours(23));
        var notificationService = Provider.GetRequiredService<NotificationService>();
        Result = await notificationService.GetNotifications();
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
        await notificationSetupUtil.SetStartTimes(DateTime.Now.AddHours(-1));
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
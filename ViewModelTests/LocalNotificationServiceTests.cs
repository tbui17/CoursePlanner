using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Plugin.LocalNotification;
using ViewModels.Services;
using ViewModelTests.TestSetup;

namespace ViewModelTests;

public class LocalNotificationServiceTests : BaseDbTest
{
    private Course _course;

    private string _courseName;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _courseName = Guid.NewGuid().ToString();
        // setup to have 1 upcoming course

        await using var db = GetDb();
        var course = db.Courses.First();
        course.Start = DateTime.Now.Date;
        course.ShouldNotify = true;
        course.Name = _courseName;
        await db.SaveChangesAsync();
        _course = course;
    }

    [Ignore("Flaky")]
    public async Task SendUpcomingNotifications_1UpcomingEvent_MessageShouldContainNotificationNameAndCount()
    {

        // subscribe to notifications

        var service = Resolve<ILocalNotificationService>();
        List<NotificationRequest> requests = [];
        service.NotificationReceived += (_, args) => requests.Add(args.Request);

        // check db for upcoming notifications and publish

        await service.SendUpcomingNotifications();

        using var _ = new AssertionScope();

        var req = requests
            .Should()
            .ContainSingle()
            .Subject;

        req
            .Title
            .Should()
            .Contain("1");
        req
            .Description
            .Should()
            .Contain(_course.Name);
    }
}
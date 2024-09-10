using FluentAssertions;
using FluentAssertions.Execution;
using FluentResults;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using Moq;
using Plugin.LocalNotification;
using ViewModels.Services;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.Services;

public class LocalNotificationServiceTests : BaseDbTest
{
    private string _courseName;

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        _courseName = Guid.NewGuid().ToString();
        // setup to have 1 upcoming course

        await using var db = await GetDb();
        var course = db.Courses.First();
        course.Start = DateTime.Now.Date;
        course.ShouldNotify = true;
        course.Name = _courseName;
        await db.SaveChangesAsync();
    }


    [Test]
    public async Task SendUpcomingNotifications_1UpcomingEvent_MessageShouldContainNotificationNameAndCount()
    {
        IUserSetting userSetting = new UserSetting() { UserId = 1, Id = 1 };
        var sessionMock = CreateMock<ISessionService>();
        sessionMock
            .Setup(x => x.GetUserSettingsAsync())
            .ReturnsAsync(userSetting.ToResult());
        var notificationMock = CreateMock<INotificationService>();
        var service = new LocalNotificationService(
            notificationDataService: Resolve<NotificationDataService>(),
            logger: Resolve<ILogger<ILocalNotificationService>>(),
            localNotificationServiceFactory: () => notificationMock.Object,
            sessionService: sessionMock.Object
        );

        // check db for upcoming notifications and publish

        var count = await service.SendUpcomingNotifications();

        using var scope = new AssertionScope();

        count.Should().Be(1);

        var subj = notificationMock.Invocations.Should()
            .ContainSingle()
            .Which.Arguments.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<NotificationRequest>()
            .Subject;

        subj.Description.Should().Contain(_courseName);

        subj.Title.Should().Contain("1");
    }
}
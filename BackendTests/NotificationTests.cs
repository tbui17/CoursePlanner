using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTests;

public abstract class NotificationFixture(DateTime time) : IAsyncLifetime
{
    public List<NotificationResult> Results { get; set; } = null!;
    public List<Course> Courses { get; set; } = null!;
    public List<Assessment> Assessments { get; set; } = null!;
    public DateTime Now { get; set; }

    public async Task SetAllNotificationStartTimes()
    {
        var factory = Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();
        await using var db = await factory.CreateDbContextAsync();

        var courses = await db
           .Courses
           .AsTracking()
           .ToListAsync();

        var assessments = await db
           .Assessments
           .AsTracking()
           .ToListAsync();

        foreach (var course in courses)
        {
            course.Start = time;
            course.ShouldNotify = true;
        }

        foreach (var assessment in assessments)
        {
            assessment.Start = time;
            assessment.ShouldNotify = true;
        }

        await db.SaveChangesAsync();
    }



    public async Task InitializeAsync()
    {

        Now = DateTime.Now;
        var notificationService = Provider.GetRequiredService<NotificationService>();

        await SetAllNotificationStartTimes();

        Results = (await notificationService.GetNotifications()).ToList();

        var entities = Results
           .Select(x => x.Entity)
           .ToList();
        Courses = entities
           .OfType<Course>()
           .ToList();
        Assessments = entities
           .OfType<Assessment>()
           .ToList();
    }

    public async Task DisposeAsync()
    {

    }
}

public class OneDayAfterFixture() : NotificationFixture(DateTime.Now.AddHours(23));




public class OneHourBeforeFixture() : NotificationFixture(DateTime.Now.AddHours(-1));




public class OneDayAfterNotificationTests : IAsyncLifetime
{
    private OneDayAfterFixture _fixture = null!;

    [Fact]
    public void ShouldHaveNotifications()
    {
        _fixture
           .Results
           .Should()
           .NotBeEmpty();
    }

    [Fact]
    public void ShouldHaveAllNotificationEntities()
    {
        using var _ = new AssertionScope();

        _fixture
           .Courses
           .Should()
           .NotBeEmpty();
        _fixture
           .Assessments
           .Should()
           .NotBeEmpty();
    }

    public async Task InitializeAsync()
    {
        _fixture = new();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}

public class OneHourBeforeNotificationTests : IAsyncLifetime
{
    private OneHourBeforeFixture _fixture = null!;

    [Fact]
    public void ShouldHaveNoNotifications()
    {
        _fixture
           .Results
           .Should()
           .BeEmpty();
    }

    public async Task InitializeAsync()
    {
        _fixture = new();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }
}
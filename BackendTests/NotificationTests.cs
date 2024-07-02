using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackendTests;

public class DbFixture : IAsyncLifetime
{
    private async Task ResetDb()
    {
        await using var db = await Provider
           .DbCtxFactory()();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public async Task InitializeAsync()
    {
        // await ResetDb();
    }

    public async Task DisposeAsync()
    {
        // await ResetDb();
    }
}

public abstract class NotificationFixture
{
    public List<NotificationResult> Results { get; set; } = null!;
    public List<Course> Courses { get; set; } = null!;
    public List<Assessment> Assessments { get; set; } = null!;
    public DateTime Now { get; set; }

    private readonly DbFixture _fixture = new();

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        Now = DateTime.Now;
        var notificationService = Provider.GetRequiredService<NotificationService>();

        await notificationService.SetAllNotificationStartTimes(Time);

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
        await _fixture.DisposeAsync();
    }

    protected abstract DateTime Time { get; }
}

public class OneDayAfterFixture : NotificationFixture, IAsyncLifetime
{
    protected override DateTime Time => Now.AddHours(23);
}

public class OneHourBeforeFixture : NotificationFixture, IAsyncLifetime
{
    protected override DateTime Time => Now.AddHours(-1);
}

public class OneDayAfterNotificationTests : IAsyncLifetime
{
    private OneDayAfterFixture _fixture = null!;

    [Fact]
    public void Should_Have_Notifications()
    {
        _fixture
           .Results
           .Should()
           .NotBeEmpty();
    }

    [Fact]
    public void Should_Have_All_Notification_Entities()
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
    public void Should_Have_No_Notifications()
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

public static class ProviderExtensions
{
    public static Func<Task<LocalDbCtx>> DbCtxFactory(this IServiceProvider provider) =>
        () => provider
           .GetRequiredService<IDbContextFactory<LocalDbCtx>>()
           .CreateDbContextAsync();
}
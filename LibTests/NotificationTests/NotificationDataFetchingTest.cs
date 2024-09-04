using FluentAssertions;
using Lib.Services.NotificationService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibTests.NotificationTests;

public class NotificationDataFetchingTest : BaseDbTest
{
    [Test]
    public async Task GetTotalItems_ReturnsCountOfAllNotifications()
    {
        await using var db = await GetDb();
        // course is 1 type of notification class
        var courseCount = await db.Courses.CountAsync();
        var notificationService = Provider.GetRequiredService<NotificationService>();
        var result = await notificationService.GetTotalItems();
        result.Should()
            .BeGreaterThan(0)
            .And.BeGreaterThan(courseCount);
    }
}
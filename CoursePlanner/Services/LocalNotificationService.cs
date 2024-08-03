using Lib.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace CoursePlanner.Services;

public interface ILocalNotificationService
{
    Task Notify();
}

public class LocalNotificationService(NotificationService notificationService, ILogger<LocalNotificationService> logger) : ILocalNotificationService
{



    public async Task Notify()
    {
        logger.LogInformation("Retrieving notifications.");
        var notifications = (await notificationService.GetNotifications()).ToList();

        logger.LogInformation("Found {NotificationsCount} notifications.", notifications.Count);

        if (notifications.Count == 0)
        {
            return;
        }

        var notificationRequest = CreateNotificationRequest(notifications);

        logger.LogInformation("Showing notification {NotificationId}: {NotificationTitle}", notificationRequest.NotificationId, notificationRequest.Title);
        await LocalNotificationCenter.Current.Show(notificationRequest);
        logger.LogInformation("Successfully sent notification.");

        return;

        static NotificationRequest CreateNotificationRequest(IReadOnlyList<NotificationResult> notifications)
        {
            var title = $"{notifications.Count} upcoming events.";
            var description = title + "\n" + notifications.ToMessage();


            return new()
            {
                NotificationId = 1,
                Title = title,
                Description = description,
                BadgeNumber = notifications.Count,
            };
        }
    }
}
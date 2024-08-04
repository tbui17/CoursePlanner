using Lib.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace ViewModels.Services;

public interface ILocalNotificationService
{
    Task Notify();
    Task Notify(NotificationRequest request);
    void StartListening();
}

public class LocalNotificationService(NotificationService notificationService, ILogger<LocalNotificationService> logger)
    : ILocalNotificationService
{
    // ReSharper disable once NotAccessedField.Local Avoid losing reference to timer
    private Timer? _notificationJob;


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

        logger.LogInformation("Showing notification {NotificationId}: {NotificationTitle}",
            notificationRequest.NotificationId, notificationRequest.Title
        );
        await Notify(notificationRequest);
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

    public async Task Notify(NotificationRequest request)
    {
        if (LocalNotificationCenter.Current is { } c)
        {
            await c.Show(request);
            return;
        }
        var msg = new
        {
            request.NotificationId,
            request.Title,
            request.Description
        };
        logger.LogWarning("LocalNotificationCenter.Current is null. Falling back to logging.");
        logger.LogInformation("Notification sent: {Notification}", msg);
    }


    public void StartListening()
    {
        var time = TimeSpan.FromDays(1);
        logger.LogInformation("Created notification timer with {Time}", time);
        _notificationJob = new Timer(
            NotifyTask,
            null,
            TimeSpan.Zero,
            time
        );
        return;
        async void NotifyTask(object? _) => await Notify();
    }
}
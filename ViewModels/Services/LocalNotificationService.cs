using Lib.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace ViewModels.Services;

public interface ILocalNotificationService
{
    Task<int> SendUpcomingNotifications();
    Task Notify(NotificationRequest request);
    void StartListening();
    event EventHandler<NotificationEventArgs>? NotificationReceived;
}

public class LocalNotificationService(
    NotificationService notificationService,
    ILogger<ILocalNotificationService> logger)
    : ILocalNotificationService
{
    // ReSharper disable once NotAccessedField.Local Avoid losing reference to timer
    private Timer? _notificationJob;

    public event EventHandler<NotificationEventArgs>? NotificationReceived;

    public async Task<int> SendUpcomingNotifications()
    {
        logger.LogInformation("Retrieving notifications.");
        var notifications = (await notificationService.GetUpcomingNotifications()).ToList();

        var notificationCount = notifications.Count;

        logger.LogInformation("Found {NotificationsCount} notifications.", notifications.Count);

        if (notificationCount == 0)
        {
            return notificationCount;
        }

        var notificationRequest = CreateNotificationRequest(notifications);

        logger.LogInformation("Showing notification {NotificationId}: {NotificationTitle}",
            notificationRequest.NotificationId, notificationRequest.Title
        );
        await Notify(notificationRequest);
        logger.LogInformation("Successfully sent notification.");

        return notificationCount;

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
        NotificationReceived?.Invoke(this, new NotificationEventArgs { Request = request });
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
        async void NotifyTask(object? _) => await SendUpcomingNotifications();
    }
}
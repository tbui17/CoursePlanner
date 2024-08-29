﻿using Lib.Services;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;

namespace ViewModels.Services;

public interface ILocalNotificationService
{
    Task<int> SendUpcomingNotifications();
    Task Notify(NotificationRequest request);
    void StartListening();
    Task RequestNotificationPermissions();
}

public class LocalNotificationService(
    NotificationService notificationService,
    ILogger<ILocalNotificationService> logger,
    Func<INotificationService?> localNotificationServiceFactory
    )
    : ILocalNotificationService
{
    private Timer? _notificationJob;

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
        var impl = localNotificationServiceFactory();
        if (impl is not null)
        {
            await impl.Show(request);
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
        if (_notificationJob is not null)
        {
            logger.LogInformation("Notification timer already exists. Refusing to create another.");
            return;
        }
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

    public async Task RequestNotificationPermissions()
    {
        if (localNotificationServiceFactory() is not { } c)
        {
            logger.LogWarning("LocalNotificationCenter.Current is null. Cannot request permissions.");
            return;
        }
        if (await c.AreNotificationsEnabled())
        {
            logger.LogInformation("Notifications are already enabled.");
            return;
        }
        logger.LogInformation("Requesting notification permissions.");
        await c.RequestNotificationPermission();
    }
}
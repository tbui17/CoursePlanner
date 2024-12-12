using Lib.Attributes;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace ViewModels.Services;

public interface ILocalNotificationService
{
    Task<int> SendUpcomingNotifications();
    Task Notify(NotificationRequest request);
    Task Startup();
}

[Inject(typeof(ILocalNotificationService), ServiceLifetime.Singleton)]
public class LocalNotificationService(
    INotificationDataService notificationDataService,
    ILogger<ILocalNotificationService> logger,
    Func<INotificationService?> localNotificationServiceFactory,
    ISessionService sessionService
)
    : ILocalNotificationService
{
    private Timer? _notificationJob;

    private bool DidRequestPermissions { get; set; }

    public async Task<int> SendUpcomingNotifications()
    {
        logger.LogInformation("Retrieving notifications.");


        var res = await sessionService
            .GetUserSettingsAsync()
            .MapAsync(notificationDataService.GetUpcomingNotifications)
            .Map(x => x.ToList());

        if (res.IsError)
        {
            var err = res.UnwrapError();
            logger.LogWarning(err, "Failed to retrieve notifications");
            return 0;
        }

        var notifications = res.Unwrap();

        logger.LogInformation("Found {NotificationsCount} notifications.", notifications.Count);

        if (notifications.Count is 0)
        {
            return 0;
        }

        var notificationRequest = CreateNotificationRequest(notifications);

        logger.LogInformation("Showing notification {NotificationId}: {NotificationTitle}",
            notificationRequest.NotificationId,
            notificationRequest.Title
        );
        await Notify(notificationRequest);
        logger.LogInformation("Successfully sent notification.");

        return notifications.Count;

        static NotificationRequest CreateNotificationRequest(IReadOnlyList<INotificationDataResult> notifications)
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

    public async Task Startup()
    {
        await RequestNotificationPermissions();
        StartListening();
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
        if (DidRequestPermissions)
        {
            logger.LogInformation("Permissions already requested.");
            return;
        }

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
        var response = await c.RequestNotificationPermission();
        logger.LogInformation("Notification permission response: {Response}", response);
        DidRequestPermissions = true;
    }
}
using Plugin.LocalNotification;
using ViewModels.Services;

namespace ViewModelTests;

public class LocalNotificationServiceTests
{
    [Test]
    public async Task METHOD()
    {
        var localNotificationService = Resolve<ILocalNotificationService>();
        await localNotificationService.Notify(new NotificationRequest
            {
                NotificationId = 2,
                Title = "Abc",
                Description = "Def"
            }
        );

        await using var db = GetDb();






    }
}
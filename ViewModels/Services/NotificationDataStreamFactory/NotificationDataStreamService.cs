using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Models;

namespace ViewModels.Services.NotificationDataStreamFactory;

[Inject]
public class NotificationDataStreamService(
    INotificationDataService notificationDataService,
    ILogger<NotificationDataStreamService> logger,
    PageResultFactory completeInputModelFactory
)
{
    public int RetryCount { get; set; } = 3;

    private IObservable<IList<INotification>> GetNotifications(IObservable<IDateTimeRange> dateFilter)
    {
        // we fetch and process data in a different thread to avoid blocking the UI thread
        // on push, starts a new subscription and switches subscribers to the new one
        // on pull, replays the result of the latest fetch cached to a buffer of size 1
        // stays alive until ref count of subscribers is 0
        return dateFilter
            .ObserveOn(RxApp.TaskpoolScheduler)
            .Select(notificationDataService.GetNotificationsWithinDateRange)
            .Switch()
            .Retry(RetryCount)
            .Replay(1)
            .RefCount()
            .Catch((Exception exception) =>
                {
                    logger.LogError(exception, "Error getting notifications: {Exception}", exception);
                    throw exception;
                }
            );
    }

    public IObservable<IPageResult> GetPageData(InputSource inputSource)
    {
        return GetNotifications(inputSource.DateFilter)
            .CombineLatest(
                inputSource.TextFilter,
                inputSource.TypeFilter,
                inputSource.PickerFilter,
                inputSource.CurrentPage,
                inputSource.PageSize
            )
            .Select(x => new NotificationsInputData
                {
                    Notifications = x.Item1,
                    InputData = new InputData
                    {
                        FilterText = x.Item2,
                        TypeFilter = x.Item3,
                        NotificationSelectedIndex = x.Item4,
                        CurrentPage = x.Item5,
                        PageSize = x.Item6,
                    }
                }
                .Thru(completeInputModelFactory.Create)
            );
    }
}
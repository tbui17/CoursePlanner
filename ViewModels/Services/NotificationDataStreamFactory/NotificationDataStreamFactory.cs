using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Models;

namespace ViewModels.Services.NotificationDataStreamFactory;

[Inject]
public class NotificationDataStreamFactory(
    INotificationDataService notificationDataService,
    ILogger<NotificationDataStreamFactory> logger,
    PageResultFactory completeInputModelFactory
)
{
    public int RetryCount { get; set; } = 3;

    private IObservable<IList<INotification>> CreateNotificationDataStream(IObservable<DateTimeRange> dateFilter,
        IObservable<object?> refresh)
    {
        return dateFilter.ObserveOn(RxApp.TaskpoolScheduler)
            .CombineLatest(refresh, (dateRange, _) => dateRange)
            .Select(notificationDataService.GetNotificationsWithinDateRange)
            .Switch()
            .Retry(RetryCount)
            .Replay(1)
            .RefCount()
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}", exception);
                throw exception;
            });
    }

    public IObservable<IPageResult> CreatePageDataStream(InputSource inputSource)
    {
        return CreateNotificationDataStream(inputSource.DateFilter, inputSource.Refresh)
            .CombineLatest(
                inputSource.TextFilter,
                inputSource.TypeFilter,
                inputSource.PickerFilter,
                inputSource.CurrentPage,
                inputSource.PageSize
            )
            .Select(x => new CompleteInputData
            {
                Notifications = x.Item1,
                FilterText = x.Item2,
                TypeFilter = x.Item3,
                NotificationSelectedIndex = x.Item4,
                CurrentPage = x.Item5,
                PageSize = x.Item6
            })
            .Select(completeInputModelFactory.Create);
    }
}
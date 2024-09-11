using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Models;

namespace ViewModels.Services;

[Inject]
public class NotificationDataStreamFactory(
    INotificationDataService notificationDataService,
    ILogger<NotificationDataStreamFactory> logger)
{

    public IObservable<IList<INotification>> CreateNotificationDataStream(IObservable<DateTimeRange> dateFilter)
    {
        return dateFilter.ObserveOn(RxApp.TaskpoolScheduler)
            .Select(notificationDataService.GetNotificationsWithinDateRange)
            .Switch();
    }

    public IObservable<List<INotification>> Create(InputSource inputSource)
    {
        return CreateNotificationDataStream(inputSource.DateFilter)
            .CombineLatest(inputSource.TextFilter, inputSource.PickerFilter, inputSource.Refresh)
            .Select(sources =>
            {
                var (notifications, (filterText, typeFilter), notificationSelectedIndex, _) = sources;

                var filterFactory = new NotificationDataFilterFactory
                {
                    FilterText = filterText,
                    TypeFilter = typeFilter,
                    SelectedNotificationOptionIndex = notificationSelectedIndex,
                };

                var filter = filterFactory.CreateFilter();

                return notifications
                    .AsParallel()
                    .Where(filter)
                    .ToList();
            })
            .Do(x => logger.LogDebug("Notification count {Count}", x.Count))
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}", exception);
                return Observable.Return(new List<INotification>());
            });
    }

    public IObservable<(List<INotification> Data, int PageCount)> Create(InputSourceWithCurrentPage source)
    {
        var paginatedDataStream = source.Data
            .Select(x => x.Chunk(10));


        var pageIndexStream = source.CurrentPage
            .Select(x => Math.Max(0, x - 1));

        var dataStream2 = pageIndexStream
            .CombineLatest(paginatedDataStream, (x, y) => (PageIndex: x, PaginatedData: y))
            .Select(x =>
            {
                var data = x.PaginatedData.ElementAtOrDefault(x.PageIndex)?.ToList() ?? [];
                var pageCount = x.PaginatedData.Count();
                return (Data: data, PageCount: pageCount);
            })
            .Do(x => logger.LogDebug("Item Data: {Data}", x));
        return dataStream2;
    }
}
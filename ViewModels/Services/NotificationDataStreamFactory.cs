using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Models;

namespace ViewModels.Services;

[Inject]
public class NotificationDataStreamFactory(INotificationDataService notificationDataService, ILogger<NotificationDataStreamFactory> logger)
{
    private static ParallelQuery<INotification> ApplyNotificationFilter(
        ParallelQuery<INotification> results,
        int notificationSelectedIndex
    ) =>
        notificationSelectedIndex switch
        {
            < 1 => results,
            1 => results.Where(item => item.ShouldNotify),
            > 1 => results.Where(item => !item.ShouldNotify)
        };

    public IObservable<List<INotification>> Create(InputSource inputSource)
    {

        return inputSource.DateFilter
            .ObserveOn(RxApp.TaskpoolScheduler)
            .SelectMany(notificationDataService.GetNotificationsWithinDateRange)
            .CombineLatest(inputSource.TextFilter, inputSource.PickerFilter, inputSource.Refresh)
            .Select(sources =>
            {
                var (notifications, (filterText, typeFilter), notificationSelectedIndex, _) = sources;

                return notifications
                    .AsParallel()
                    .Thru(notificationStream => ApplyNotificationFilter(notificationStream, notificationSelectedIndex))
                    .Where(item => item.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
                    .Where(item =>
                        item is Assessment assessment
                            ? $"{assessment.Type} Assessment".Contains(typeFilter,
                                StringComparison.CurrentCultureIgnoreCase)
                            : item.GetType().Name.Contains(typeFilter, StringComparison.CurrentCultureIgnoreCase));
            })
            .Select(x => x.ToList())
            .Do(x => logger.LogDebug("Notification count {Count}", x.Count))
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}",exception);
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
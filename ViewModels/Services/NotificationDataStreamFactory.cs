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

    private IObservable<IList<INotification>> CreateNotificationDataStream(IObservable<DateTimeRange> dateFilter, IObservable<object?> refresh)
    {
        return dateFilter.ObserveOn(RxApp.TaskpoolScheduler)
            .CombineLatest(refresh, (dateRange,_) => dateRange)
            .Select(notificationDataService.GetNotificationsWithinDateRange)
            .Switch();
    }

    public (IObservable<List<INotification>> Data, IObservable<int> PageCount, IObservable<int> ItemCount) Create(InputSource inputSource)
    {
        var res = CreateNotificationDataStream(inputSource.DateFilter, inputSource.Refresh)
            .CombineLatest(inputSource.TextFilter, inputSource.TypeFilter, inputSource.PickerFilter, inputSource.CurrentPage,inputSource.PageSize)
            .Select(sources =>
            {
                var (notifications,filterText, typeFilter, notificationSelectedIndex, currentPage, pageSize) = sources;
                var pageIndex = Math.Max(0, currentPage - 1);
                var partitionSize = Math.Max(1, pageSize);

                var filteredData = FilterData();
                var paginatedData = PaginateData();

                var currentPageItems = GetCurrentPage();

                return (Data: currentPageItems, PageCount: paginatedData.Count);

                List<INotification[]> PaginateData()
                {
                    var notificationsList = filteredData.Chunk(partitionSize).ToList();
                    return notificationsList;
                }

                ParallelQuery<INotification> FilterData()
                {
                    var filterFactory = new NotificationDataFilterFactory { FilterText = filterText, TypeFilter = typeFilter, SelectedNotificationOptionIndex = notificationSelectedIndex, };
                    var filter = filterFactory.CreateFilter();
                    var parallelQuery = notifications.AsParallel().Where(filter);
                    return parallelQuery;
                }

                INotification[] GetCurrentPage()
                {
                    return paginatedData.ElementAtOrDefault(pageIndex) ?? [];
                }
            })
            .Do(x => logger.LogDebug("Notification count {Count}", x.Data.Length))
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}", exception);
                (INotification[] Data, int PageCount) empty = ([], 0);
                return Observable.Return(empty);
            });

        return (
            Data: res.Select(x => x.Data.ToList()),
            PageCount: res.Select(x => x.PageCount),
            ItemCount: res.Select(x => x.Data.Length)
        );
    }
}
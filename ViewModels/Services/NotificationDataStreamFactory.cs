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
    private IObservable<IList<INotification>> CreateNotificationDataStream(IObservable<DateTimeRange> dateFilter,
        IObservable<object?> refresh)
    {
        return dateFilter.ObserveOn(RxApp.TaskpoolScheduler)
            .CombineLatest(refresh, (dateRange, _) => dateRange)
            .Select(notificationDataService.GetNotificationsWithinDateRange)
            .Switch();
    }

    public PageDataStream CreatePageDataStream(InputSource inputSource)
    {
        var combinedSource = CreateNotificationDataStream(inputSource.DateFilter, inputSource.Refresh)
            .CombineLatest(
                inputSource.TextFilter,
                inputSource.TypeFilter,
                inputSource.PickerFilter,
                inputSource.CurrentPage,
                inputSource.PageSize
            )
            .Select(x => new CombinedSource
            {
                Notifications = x.Item1,
                FilterText = x.Item2,
                TypeFilter = x.Item3,
                NotificationSelectedIndex = x.Item4,
                CurrentPage = x.Item5,
                PageSize = x.Item6
            });


        var combinedResult = combinedSource
            .Select(Selector)
            .Do(x => logger.LogDebug("Notification count {Count}", x.Data.Count))
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}", exception);
                return Observable.Return(new CombinedResult());
            });

        return new PageDataStream(
            Data: combinedResult.Select(x => x.Data),
            PageCount: combinedResult.Select(x => x.PageCount),
            ItemCount: combinedResult.Select(x => x.Data.Count)
        );
    }

    private static CombinedResult Selector(CombinedSource sources)
    {
        var (notifications, filterText, typeFilter, notificationSelectedIndex, currentPage, pageSize) = sources;
        var pageIndex = Math.Max(0, currentPage - 1);
        var partitionSize = Math.Max(1, pageSize);

        var filteredData = FilterData();
        var paginatedData = PaginateData();

        var currentPageItems = GetCurrentPage();

        return new CombinedResult
        {
            Data = currentPageItems.ToList(),
            PageCount = paginatedData.Count
        };

        List<INotification[]> PaginateData()
        {
            var notificationsList = filteredData.Chunk(partitionSize).ToList();
            return notificationsList;
        }

        ParallelQuery<INotification> FilterData()
        {
            var filterFactory = new NotificationDataFilterFactory
            {
                FilterText = filterText, TypeFilter = typeFilter,
                SelectedNotificationOptionIndex = notificationSelectedIndex,
            };
            var filter = filterFactory.CreateFilter();
            var parallelQuery = notifications.AsParallel().Where(filter);
            return parallelQuery;
        }

        INotification[] GetCurrentPage()
        {
            return paginatedData.ElementAtOrDefault(pageIndex) ?? [];
        }
    }
}
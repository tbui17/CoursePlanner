using System.Reactive.Linq;

using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Domain;
using ViewModels.Models;

namespace ViewModels.Services;

[Inject]
public class NotificationDataStreamFactory(
    INotificationDataService notificationDataService,
    ILogger<NotificationDataStreamFactory> logger)
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
            .Catch((Exception exception) =>
            {
                logger.LogError(exception, "Error getting notifications: {Exception}", exception);
                return Observable.Return(new List<INotification>());
            });
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
            .Select(x => new CompleteInputSource
            {
                Notifications = x.Item1,
                FilterText = x.Item2,
                TypeFilter = x.Item3,
                NotificationSelectedIndex = x.Item4,
                CurrentPage = x.Item5,
                PageSize = x.Item6
            });


        var data = combinedSource.Select(x => x.CreateFilteredPaginatedData());

        return new PageDataStream
        {
            Data = data.Select(x => x.Data),
            PageCount = data.Select(x => x.PageCount),
            ItemCount = data.Select(x => x.Data.Count)
        };
    }


}
file class CompleteInputSource
{
    public required IList<INotification> Notifications { get; init; }
    public required string FilterText { get; init; }
    public required string TypeFilter { get; init; }
    public required ShouldNotifyIndex NotificationSelectedIndex { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }

    public void Deconstruct(out IList<INotification> notifications, out string filterText, out string typeFilter,
        out ShouldNotifyIndex notificationSelectedIndex, out int currentPage, out int pageSize)
    {
        notifications = Notifications;
        filterText = FilterText;
        typeFilter = TypeFilter;
        notificationSelectedIndex = NotificationSelectedIndex;
        currentPage = CurrentPage;
        pageSize = PageSize;
    }

    public FilteredPaginatedData CreateFilteredPaginatedData()
    {
        var (notifications, filterText, typeFilter, notificationSelectedIndex, currentPage, pageSize) = this;
        // apply constraints
        var pageIndex = Math.Max(0, currentPage - 1);
        var partitionSize = Math.Max(1, pageSize);


        // filter and transform
        var filteredData = FilterData();
        var paginatedData = PaginateData();

        var currentPageItems = GetCurrentPage();

        return new FilteredPaginatedData
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


file record FilteredPaginatedData
{
    public List<INotification> Data { get; init; } = [];
    public int PageCount { get; init; }
}
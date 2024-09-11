using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using ViewModels.Models;

namespace ViewModels.Services.NotificationDataStreamFactory;

[Inject]
public class PageDataFactory
{
    public PageDataStream CreateStream(IObservable<PageData> data)
    {
        return new PageDataStream
        {
            Data = data.Select(x => x.Data),
            PageCount = data.Select(x => x.PageCount),
            ItemCount = data.Select(x => x.Data.Count)
        };
    }

    public PageData Create(CompleteInputData input)
    {
        var (notifications, filterText, typeFilter, notificationSelectedIndex, currentPage, pageSize) = input;
        // apply constraints
        var pageIndex = Math.Max(0, currentPage - 1);
        var partitionSize = Math.Max(1, pageSize);


        // filter and transform
        var filteredData = FilterData();
        var paginatedData = PaginateData();

        var currentPageItems = GetCurrentPage();

        return new PageData
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
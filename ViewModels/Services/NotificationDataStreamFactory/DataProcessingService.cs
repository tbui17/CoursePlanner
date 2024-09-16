using Lib.Interfaces;

namespace ViewModels.Services.NotificationDataStreamFactory;

public class DataProcessingService(IPaginationDetails details, IList<INotification> notifications, Func<INotification, bool> filter)
{
    private IReadOnlyList<INotification>? _pageData;
    public IReadOnlyList<INotification> CurrentPageData => _pageData ??= GetCurrentPage();
    public int ItemCount => CurrentPageData.Count;


    private ParallelQuery<INotification> GetFilteredData()
    {
        return notifications
            .AsParallel()
            .Where(filter)
            .OrderBy(x => x.Id);
    }

    private IEnumerable<INotification[]> GetPaginatedData() =>
        GetFilteredData()
            .Chunk(details.PageSize);

    private INotification[] GetCurrentPage() => GetPaginatedData()
        .ElementAtOrDefault(details.Index) ?? [];
}
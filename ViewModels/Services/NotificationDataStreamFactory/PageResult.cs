using Lib.Attributes;
using Lib.Interfaces;
using Microsoft.Extensions.Logging;

namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IPageResult
{
    int PageCount { get; }
    int ItemCount { get; }
    int CurrentPage { get; }
    IReadOnlyList<INotification> CurrentPageData { get; }
    bool HasNext { get; }
    bool HasPrevious { get; }
}

public record EmptyPageResult : IPageResult
{
    public int PageCount { get; } = 0;
    public int ItemCount { get; } = 0;
    public int CurrentPage { get; } = 1;
    public IReadOnlyList<INotification> CurrentPageData { get; } = [];
    public bool HasNext { get; } = false;
    public bool HasPrevious { get; } = false;
}

public class PageResult(PaginationModel model, FilteredDataResultProvider filteredDataResultProvider) : IPageResult
{
    // private readonly ImplHelper _helper;

    public int PageCount => model.PageCount;
    public bool HasNext => model.HasNext;
    public bool HasPrevious => model.HasPrevious;
    public int CurrentPage => model.CurrentPage;

    public IReadOnlyList<INotification> CurrentPageData => filteredDataResultProvider.CurrentPageData;
    public int ItemCount => filteredDataResultProvider.ItemCount;


}

public class FilteredDataResultProvider(IPaginationDetails details, IList<INotification> notifications, Func<INotification, bool> filter)
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

[Inject]
public class PageResultFactory(ILogger<PageResultFactory> logger)
{
    public PageResult Create(ReturnedData data)
    {
        var model = new PaginationModel
        {
            Count = data.Notifications.Count,
            Index = data.InputData.CurrentPage - 1,
            PageSize = data.InputData.PageSize
        };
        logger.LogDebug("Creating PageResult for {data}", data);
        var factory = new NotificationDataFilterFactory(data.InputData);
        var filter = factory.CreateFilter();
        var dataProvider = new FilteredDataResultProvider(model, data.Notifications, filter);

        return new PageResult(model, dataProvider);
    }
}
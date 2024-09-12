using Lib.Attributes;
using Lib.Interfaces;
using Lib.Utils;

namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IPageResult
{
    int PageCount { get; }
    int ItemCount { get; }
    int CurrentPage { get; }
    IReadOnlyList<INotification> CurrentPageData { get; }
}

public record EmptyPageResult : IPageResult
{
    public int PageCount { get; init; } = 0;
    public int ItemCount { get; init; } = 0;
    public int CurrentPage { get; init; } = 0;
    public IReadOnlyList<INotification> CurrentPageData { get; init; } = [];
}

public class PageResult(Func<INotification, bool> filter, CompleteInputData data) : IPageResult
{
    private CompleteInputData Data => data;
    private int PageIndex => Math.Max(0, Data.CurrentPage - 1);
    private int PartitionSize => Math.Max(1, Data.PageSize);
    public int PageCount => Data.Notifications.Count.DivideRoundedUp(PartitionSize);
    public int TotalItemCount => Data.Notifications.Count;
    public int CurrentPage => Data.CurrentPage;
    private IReadOnlyList<INotification>? _pageData;
    public IReadOnlyList<INotification> CurrentPageData => _pageData ??= GetCurrentPage();
    public int ItemCount => CurrentPageData.Count;

    private ParallelQuery<INotification> GetFilteredData()
    {
        return Data.Notifications.AsParallel().Where(filter).OrderBy(x => x.Id);
    }

    private List<INotification[]> GetPaginatedData() =>
        GetFilteredData()
            .Chunk(PartitionSize)
            .ToList();

    private IReadOnlyList<INotification> GetCurrentPage() => GetPaginatedData()
        .ElementAtOrDefault(PageIndex) ?? [];
}



[Inject]
public class PageResultFactory
{
    public PageResult Create(CompleteInputData data)
    {
        var factory = new NotificationDataFilterFactory(data);
        var filter = factory.CreateFilter();
        return new PageResult(filter, data);
    }
}
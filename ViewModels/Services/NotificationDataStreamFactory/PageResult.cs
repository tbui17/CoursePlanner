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
    public int PageCount { get; } = 0;
    public int ItemCount { get; } = 0;
    public int CurrentPage { get; } = 1;
    public IReadOnlyList<INotification> CurrentPageData { get; } = [];
}

public class PageResult(Func<INotification, bool> filter, ReturnedData data) : IPageResult
{
    internal ReturnedData Data => data;
    internal int PageIndex => Math.Max(0, Data.CurrentPage - 1);
    internal int PartitionSize => Math.Max(1, Data.PageSize);
    public int PageCount => TotalItemCount.DivideRoundedUp(PartitionSize);
    internal int TotalItemCount => Data.Notifications.Count;
    public int CurrentPage => Data.CurrentPage;
    private IReadOnlyList<INotification>? _pageData;
    public IReadOnlyList<INotification> CurrentPageData => _pageData ??= GetCurrentPage();
    public int ItemCount => CurrentPageData.Count;

    internal ParallelQuery<INotification> GetFilteredData()
    {
        return Data.Notifications.AsParallel().Where(filter).OrderBy(x => x.Id);
    }

    internal List<INotification[]> GetPaginatedData() =>
        GetFilteredData()
            .Chunk(PartitionSize)
            .ToList();

    private IReadOnlyList<INotification> GetCurrentPage() => GetPaginatedData()
        .ElementAtOrDefault(PageIndex) ?? [];
}



[Inject]
public class PageResultFactory
{
    public PageResult Create(ReturnedData data)
    {
        var factory = new NotificationDataFilterFactory(data);
        var filter = factory.CreateFilter();
        return new PageResult(filter, data);
    }
}
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Utils;
using Microsoft.Extensions.Logging;

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
    internal IList<INotification> DataSource => Data.Notifications;
    internal int PageIndex => Math.Max(0, Data.CurrentPage - 1);
    internal int PartitionSize => Math.Max(1, Data.PageSize);
    public int PageCount => TotalItemCount.DivideRoundedUp(PartitionSize);
    internal int TotalItemCount => DataSource.Count;
    public int CurrentPage => Data.CurrentPage;
    private IReadOnlyList<INotification>? _pageData;
    public IReadOnlyList<INotification> CurrentPageData => _pageData ??= GetCurrentPage();
    public int ItemCount => CurrentPageData.Count;

    private ParallelQuery<INotification> GetFilteredData()
    {
        return Data.Notifications.AsParallel().Where(filter).OrderBy(x => x.Id);
    }

    private IEnumerable<INotification[]> GetPaginatedData() =>
        GetFilteredData()
            .Chunk(PartitionSize);

    private INotification[] GetCurrentPage() => GetPaginatedData()
        .ElementAtOrDefault(PageIndex) ?? [];
}

[Inject]
public class PageResultFactory(ILogger<PageResultFactory> logger)
{
    public PageResult Create(ReturnedData data)
    {
        logger.LogDebug("Creating PageResult for {data}", data);
        var factory = new NotificationDataFilterFactory(data);
        var filter = factory.CreateFilter();

        return new PageResult(filter, data);
    }
}
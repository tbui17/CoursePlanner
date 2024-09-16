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

public class PageResult : IPageResult
{
    public PageResult(Func<INotification, bool> filter, ReturnedData data)
    {
        _helper = new ImplHelper(data);
        _helper2 = new ImplHelper2(_helper, data.Notifications, filter);
    }

    private readonly ImplHelper _helper;
    private readonly ImplHelper2 _helper2;

    public int PageCount => _helper.PageCount;
    internal int TotalItemCount => _helper.TotalItemCount;
    internal int PartitionSize => _helper.PartitionSize;
    public bool HasNext => _helper.HasNext;
    public bool HasPrevious => _helper.HasPrevious;
    public int CurrentPage => _helper.CurrentPage;

    public IReadOnlyList<INotification> CurrentPageData => _helper2.CurrentPageData;
    public int ItemCount => _helper2.ItemCount;

    private class ImplHelper(ReturnedData data)
    {
        internal int PartitionSize => Math.Max(1, data.InputData.PageSize);
        internal int CurrentPage => data.InputData.CurrentPage;
        internal int TotalItemCount => data.Notifications.Count;
        internal int PageIndex => Math.Max(0, CurrentPage - 1);
        internal int PageCount => TotalItemCount.DivideRoundedUp(PartitionSize);
        internal bool HasNext => PageIndex < PageCount - 1;
        internal bool HasPrevious => PageIndex > 0;
    }

    private class ImplHelper2(ImplHelper helper, IList<INotification> notifications, Func<INotification, bool> filter)
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
                .Chunk(helper.PartitionSize);

        private INotification[] GetCurrentPage() => GetPaginatedData()
            .ElementAtOrDefault(helper.PageIndex) ?? [];
    }
}

[Inject]
public class PageResultFactory(ILogger<PageResultFactory> logger)
{
    public PageResult Create(ReturnedData data)
    {
        logger.LogDebug("Creating PageResult for {data}", data);
        var factory = new NotificationDataFilterFactory(data.InputData);
        var filter = factory.CreateFilter();

        return new PageResult(filter, data);
    }
}
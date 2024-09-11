using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Utils;
using ViewModels.Models;

namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IPageResult
{
    int PageCount { get; }
    int ItemCount { get; }
    int CurrentPage { get; }
    IReadOnlyList<INotification> CurrentPageData { get; }
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
        return Data.Notifications.AsParallel().Where(filter);
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

public static class PageResultExtensions
{
    public static PageDataStream CreatePageDataStream(this IObservable<PageResult> stream)
    {

        return new PageDataStream
        {
            Data = stream.Select(x => x.CurrentPageData.ToList()),
            PageCount = stream.Select(x => x.PageCount),
            ItemCount = stream.Select(x => x.ItemCount)
        };
    }
}
using System.Reactive.Linq;
using Lib.Interfaces;
using ViewModels.Domain;
using ViewModels.Models;

namespace ViewModels.Services.NotificationDataStreamFactory;

public class CompleteInputModel
{
    public CompleteInputData Data { get; init; } = new();

    public int PageIndex => Math.Max(0, Data.CurrentPage - 1);
    public int PartitionSize => Math.Max(1, Data.PageSize);

    private IReadOnlyList<INotification>? _cache;

    public IReadOnlyList<INotification> GetFilteredData()
    {
        if (_cache is not null)
        {
            return _cache;
        }
        var filter = new NotificationDataFilterFactory { Data = Data }.CreateFilter();
        _cache =  Data.Notifications.AsParallel().Where(filter).ToList();
        return _cache;
    }

    public IReadOnlyList<IReadOnlyList<INotification>> GetPaginatedData() =>
        GetFilteredData()
            .Chunk(PartitionSize)
            .ToList();


    public IReadOnlyList<INotification> GetCurrentPage() => GetPaginatedData()
        .ElementAtOrDefault(PageIndex) ?? [];
}

public record CompleteInputData : IFilterData
{
    public IList<INotification> Notifications { get; init; } = [];
    public string FilterText { get; init; } = "";
    public string TypeFilter { get; init; } = "";
    public ShouldNotifyIndex NotificationSelectedIndex { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }

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
}

public static class CompleteInputModelExtensions
{
    public static PageDataStream CreatePageDataStream(this IObservable<CompleteInputModel> stream)
    {
        return new PageDataStream
        {
            Data = stream.Select(x => x.GetCurrentPage().ToList()),
            PageCount = stream.Select(x => x.GetPaginatedData().Count),
            ItemCount = stream.Select(x => x.GetCurrentPage().Count)
        };
    }
}
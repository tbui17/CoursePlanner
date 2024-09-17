using Lib.Interfaces;

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

internal record PageResult : IPageResult
{
    public PageResult(PaginationModel model, IReadOnlyList<INotification> data)
    {
        PageCount = model.PageCount;
        HasNext = model.HasNext;
        HasPrevious = model.HasPrevious;
        CurrentPage = model.CurrentPage;
        CurrentPageData = data;
        ItemCount = data.Count;
    }

    public int PageCount { get; }
    public bool HasNext { get; }
    public bool HasPrevious { get; }
    public int CurrentPage { get; }
    public IReadOnlyList<INotification> CurrentPageData { get; }
    public int ItemCount { get; }
}
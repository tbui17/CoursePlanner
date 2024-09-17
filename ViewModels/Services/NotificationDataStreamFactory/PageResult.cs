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

internal class PageResult(PaginationModel model, IReadOnlyList<INotification> data) : IPageResult
{
    // private readonly ImplHelper _helper;

    public int PageCount => model.PageCount;
    public bool HasNext => model.HasNext;
    public bool HasPrevious => model.HasPrevious;
    public int CurrentPage => model.CurrentPage;

    public IReadOnlyList<INotification> CurrentPageData => data;
    public int ItemCount => data.Count;


}
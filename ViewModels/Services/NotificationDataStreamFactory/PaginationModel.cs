namespace ViewModels.Services.NotificationDataStreamFactory;

public class PaginationModel : IPaginationDetails
{
    private readonly int _index;
    public int Index
    {
        get => _index;
        init => _index = Math.Max(0, value);
    }
    private readonly int _count;
    public int Count
    {
        get => _count;
        init => _count = Math.Max(0, value);
    }

    private readonly int _pageSize;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = Math.Max(1, value);
    }

    public int CurrentPage => Index + 1;
    public int PageCount => (Count + PageSize - 1) / PageSize;
    public int LastPageIndex => PageCount - 1;
    public bool IsFirstPage => Index == 0;
    public bool IsLastPage => Index == LastPageIndex;
    public bool HasNext => Index < LastPageIndex;
    public bool HasPrevious => Index > 0;
}
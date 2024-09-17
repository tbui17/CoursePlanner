namespace ViewModels.Services.NotificationDataStreamFactory;

public class PaginationModel : IPaginationDetails
{

    public static PaginationModel Create(int index, int count, int pageSize)
    {

        if (count == 0 || pageSize == 0)
        {
            index = 0;
        }


        return new PaginationModel
        {
            Index = index,
            Count = count,
            PageSize = pageSize
        };
    }

    private PaginationModel(){}


    private readonly int _index;

    public int Index
    {
        get => _index;
        private init => _index = Math.Max(0, value);
    }

    private readonly int _count;

    public int Count
    {
        get => _count;
        private init => _count = Math.Max(0, value);
    }

    private readonly int _pageSize;

    public int PageSize
    {
        get => _pageSize;
        private init => _pageSize = Math.Max(1, value);
    }

    public int CurrentPage => Index + 1;
    public int PageCount
    {
        get
        {
            var pageCount = (Count + PageSize - 1) / PageSize;
            return Math.Max(1, pageCount);
        }
    }

    public int LastPageIndex => Math.Max(0, PageCount - 1);
    public bool IsFirstPage => Index == 0;
    public bool IsLastPage => Index == LastPageIndex;
    public bool HasNext => Index < LastPageIndex;
    public bool HasPrevious => Index > 0;

    public override string ToString()
    {
        return new
        {
            Index,
            Count,
            PageSize,
            PageCount,
            CurrentPage,
            LastPageIndex,
            IsFirstPage,
            IsLastPage,
            HasNext,
            HasPrevious
        }.ToString() ?? "";
    }
}
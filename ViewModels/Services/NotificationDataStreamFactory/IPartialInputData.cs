namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IPartialInputData : IFilterData
{
    public int CurrentPage { get; }
    public int PageSize { get; }
}
using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IFilterData
{
    public string FilterText { get; }
    public string TypeFilter { get; }
    public ShouldNotifyIndex NotificationSelectedIndex { get; }
}

public interface IInputData : IFilterData
{
    public int CurrentPage { get; }
    public int PageSize { get; }
}
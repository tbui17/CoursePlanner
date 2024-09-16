using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModels.Services.NotificationDataStreamFactory;

public interface IFilterData
{
    public string FilterText { get; }
    public string TypeFilter { get; }
    public ShouldNotifyIndex NotificationSelectedIndex { get; }
}
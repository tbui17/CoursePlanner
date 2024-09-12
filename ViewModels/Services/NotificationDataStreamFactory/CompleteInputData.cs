using Lib.Interfaces;
using ViewModels.Domain;
using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModels.Services.NotificationDataStreamFactory;

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
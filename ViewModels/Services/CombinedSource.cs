using Lib.Interfaces;
using ViewModels.Domain;

namespace ViewModels.Services;

public record CombinedSource
{
    public required IList<INotification> Notifications { get; init; }
    public required string FilterText { get; init; }
    public required string TypeFilter { get; init; }
    public required ShouldNotifyIndex NotificationSelectedIndex { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }

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
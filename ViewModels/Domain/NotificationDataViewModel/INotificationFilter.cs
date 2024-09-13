using Lib.Interfaces;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationFilter : IDateTimeRange
{
    string FilterText { get; set; }
    string TypeFilter { get; set; }
    IList<string> NotificationOptions { get; }
    ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }
    int CurrentPage { get; set; }
    int PageSize { get; set; }
}
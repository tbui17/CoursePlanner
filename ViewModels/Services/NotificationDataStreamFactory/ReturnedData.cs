using Lib.Interfaces;
using Lib.Models;
using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModels.Services.NotificationDataStreamFactory;



public record ReturnedData
{
    public IList<INotification> Notifications { get; init; } = [];
    public IPartialInputData InputData { get; init; } = new PartialInputData();
    public int Index => InputData.CurrentPage - 1;


}

public record PartialInputData : IPartialInputData
{
    public string FilterText { get; init; } = "";
    public string TypeFilter { get; init; } = "";
    public ShouldNotifyIndex NotificationSelectedIndex { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
}

public record InputData : IPartialInputData
{
    public string FilterText { get; init; } = "";
    public string TypeFilter { get; init; } = "";
    public ShouldNotifyIndex NotificationSelectedIndex { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public IDateTimeRange DateRange { get; init; } = new DateTimeRange();
}
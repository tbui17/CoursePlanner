using Lib.Interfaces;
using Lib.Models;
using ViewModels.Domain;
using ViewModels.Domain.NotificationDataViewModel;

namespace ViewModels.Models;

public record InputSource
{
    public required IObservable<IDateTimeRange> DateFilter { get; init; }
    public required IObservable<string> TextFilter { get; init; }
    public required IObservable<string> TypeFilter { get; init; }
    public required IObservable<ShouldNotifyIndex> PickerFilter { get; init; }
    public required IObservable<int> CurrentPage { get; init; }
    public required IObservable<int> PageSize { get; init; }
}
using Lib.Models;
using ViewModels.Domain;

namespace ViewModels.Models;

public class InputSource
{
    public required IObservable<DateTimeRange> DateFilter { get; init; }
    public required IObservable<string> TextFilter { get; init; }
    public required IObservable<string> TypeFilter { get; init; }
    public required IObservable<ShouldNotifyIndex> PickerFilter { get; init; }
    public required IObservable<object?> Refresh { get; init; }
    public required IObservable<int> CurrentPage { get; init; }
    public required IObservable<int> PageSize { get; init; }
}
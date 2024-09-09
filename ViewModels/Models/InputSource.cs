using System.Reactive.Subjects;
using Lib.Models;

namespace ViewModels.Models;

public record InputSource
{
    public required IObservable<DateTimeRange> DateFilterSource { get; init; }
    public required IObservable<TextFilterSource> TextFilterSource { get; init; }
    public required IObservable<int> PickerFilterSource { get; init; }
    public required BehaviorSubject<object?> RefreshSource { get; init; }
}

public record TextFilterSource(string TextFilter, string TypeFilter);
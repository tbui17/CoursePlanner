using System.Reactive.Subjects;
using Lib.Models;

namespace ViewModels.Models;

public record InputSource(
    IObservable<DateTimeRange> DateFilterSource,
    IObservable<(string, string)> TextFilterSource,
    IObservable<int> PickerFilterSource,
    BehaviorSubject<object?> RefreshSource
);
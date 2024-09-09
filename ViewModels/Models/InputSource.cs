using System.Reactive.Subjects;
using Lib.Models;

namespace ViewModels.Models;

public record InputSource(
    IObservable<DateTimeRange> DateFilterSource,
    IObservable<TextFilterSource> TextFilterSource,
    IObservable<int> PickerFilterSource,
    BehaviorSubject<object?> RefreshSource
);

public record TextFilterSource(string TextFilter, string TypeFilter);
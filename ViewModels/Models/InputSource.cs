using System.Reactive.Subjects;
using Lib.Interfaces;
using Lib.Models;

namespace ViewModels.Models;

public class InputSource
{
    public required IObservable<DateTimeRange> DateFilter { get; init; }
    public required IObservable<TextFilterSource> TextFilter { get; init; }
    public required IObservable<int> PickerFilter { get; init; }
    public required IObservable<object?> Refresh { get; init; }
}

public class InputSourceWithCurrentPage
{
    public required IObservable<List<INotification>> Data { get; init; }
    public required IObservable<int> CurrentPage { get; init; }

}

public record TextFilterSource(string TextFilter, string TypeFilter);
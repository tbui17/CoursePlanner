using ReactiveUI;
using ViewModels.Models;

namespace ViewModels.Services.NotificationDataStreamFactory;

public class InputSourceFactory
{
    public required InputData Data { get; init; }

    public InputSource CreateReactiveProperties() =>
        new()
        {
            CurrentPage = Data.CurrentPage.ToReactiveProperty(),
            DateFilter = Data.DateRange.ToReactiveProperty()!,
            PageSize = Data.PageSize.ToReactiveProperty(),
            TextFilter = Data.FilterText.ToReactiveProperty()!,
            PickerFilter = Data.NotificationSelectedIndex.ToReactiveProperty(),
            TypeFilter = Data.TypeFilter.ToReactiveProperty()!
        };
}

public static class ReactivePropertyExtensions
{
    public static ReactiveProperty<T> ToReactiveProperty<T>(this T value) => new(value);
}
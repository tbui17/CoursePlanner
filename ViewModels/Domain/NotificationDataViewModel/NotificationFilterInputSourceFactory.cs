using System.Reactive.Linq;
using Lib.Attributes;
using Lib.Models;
using ReactiveUI;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain.NotificationDataViewModel;

[Inject]
public class PageResultStreamFactory(NotificationDataStreamFactory factory)
{
    public IObservable<IPageResult> Create(INotificationFilter fields, IObservable<object?> refreshSource)
    {
        var source = new NotificationFilterInputSourceFactory(fields).CreateInputSource(refreshSource);
        return factory.CreatePageDataStream(source);
    }
}

public class NotificationFilterInputSourceFactory(INotificationFilter viewModel)
{
    public InputSource CreateInputSource(IObservable<object?> refreshSubject)
    {
        var (textFilter, typeFilter) = CreateTextFilters();
        var inputSource = new InputSource
        {
            DateFilter = CreateDateFilterSource(),
            TextFilter = textFilter,
            TypeFilter = typeFilter,
            PickerFilter = CreatePickerFilterSource(),
            CurrentPage = viewModel.WhenAnyValue(x => x.CurrentPage),
            Refresh = refreshSubject,
            PageSize = viewModel.WhenAnyValue(x => x.PageSize)
        };
        return inputSource;
    }

    public IObservable<DateTimeRange> CreateDateFilterSource()
    {
        var dateFilterSource = viewModel
            .WhenAnyValue(
                vm => vm.Start,
                vm => vm.End,
                (start, end) => new DateTimeRange { Start = start, End = end }
            );
        return dateFilterSource;
    }

    public IObservable<ShouldNotifyIndex> CreatePickerFilterSource()
    {
        var pickerFilterSource = viewModel.WhenAnyValue(x => x.SelectedNotificationOptionIndex);
        return pickerFilterSource;
    }

    public (IObservable<string> TextFilter, IObservable<string> TypeFilter) CreateTextFilters()
    {
        var textFilterSource = viewModel.WhenAnyValue(
                vm => vm.FilterText,
                vm => vm.TypeFilter
            )
            .Throttle(TimeSpan.FromMilliseconds(500));
        return (textFilterSource.Select(x => x.Item1), textFilterSource.Select(x => x.Item2));
    }
}
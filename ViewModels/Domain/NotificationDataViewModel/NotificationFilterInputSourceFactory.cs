using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Attributes;
using Lib.Models;
using ReactiveUI;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain.NotificationDataViewModel;

[Inject]
public class NotificationFilterService(NotificationDataStreamFactory factory)
{
    private readonly BehaviorSubject<Unit> _refresh = new(Unit.Default);

    public IObservable<IPageResult> Connect(INotificationFilter fields)
    {
        var source = new NotificationFilterInputSourceFactory(fields).CreateInputSource();

        return _refresh
            .Select(_ => factory.CreatePageDataStream(source))
            .Switch();
    }

    public void Refresh()
    {
        _refresh.OnNext(_refresh.Value);
    }
}

public class NotificationFilterInputSourceFactory(INotificationFilter viewModel)
{
    public InputSource CreateInputSource()
    {
        var (textFilter, typeFilter) = CreateTextFilters();
        var inputSource = new InputSource
        {
            DateFilter = CreateDateFilterSource(),
            TextFilter = textFilter,
            TypeFilter = typeFilter,
            PickerFilter = CreatePickerFilterSource(),
            CurrentPage = viewModel.WhenAnyValue(x => x.CurrentPage),
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
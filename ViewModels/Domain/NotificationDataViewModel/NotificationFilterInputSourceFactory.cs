using System.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Interfaces;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain.NotificationDataViewModel;

public class NotificationFilterViewModel : ReactiveObject, INotificationFilter, IRefresh
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string FilterText { get; set; } = "";
    public string TypeFilter { get; set; } = "";
    public IList NotificationOptions { get; set; } = new ArrayList();
    public ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }

    public NotificationFilterViewModel(
        IDefaultDateProvider dateProvider,
        IDefaultPageProvider pageProvider,
        NotificationDataStreamFactory factory,
        ILogger<NotificationFilterViewModel> logger
    )
    {
        var range = dateProvider.DateRange;
        Start = range.Start;
        End = range.End;
        PageSize = pageProvider.PageSize;
        _logger = logger;

        factory.CreatePageDataStream(CreateInputSource());
    }


    private readonly BehaviorSubject<object?> _refreshSubject = new(new object());
    private readonly ILogger<NotificationFilterViewModel> _logger;


    public Task RefreshAsync()
    {
        _logger.LogInformation("Refreshing data");
        _refreshSubject.OnNext(new object());
        return Task.CompletedTask;
    }


    private InputSource CreateInputSource()
    {
        var (textFilter, typeFilter) = CreateTextFilters();
        var inputSource = new InputSource
        {
            DateFilter = CreateDateFilterSource(),
            TextFilter = textFilter,
            TypeFilter = typeFilter,
            PickerFilter = CreatePickerFilterSource(),
            CurrentPage = this.WhenAnyValue(x => x.CurrentPage),
            Refresh = _refreshSubject,
            PageSize = this.WhenAnyValue(x => x.PageSize)
        };
        return inputSource;
    }

    private IObservable<DateTimeRange> CreateDateFilterSource()
    {
        var dateFilterSource = this
            .WhenAnyValue(
                vm => vm.Start,
                vm => vm.End,
                (start, end) => new DateTimeRange { Start = start, End = end }
            );
        return dateFilterSource;
    }

    private IObservable<ShouldNotifyIndex> CreatePickerFilterSource()
    {
        var pickerFilterSource = this.WhenAnyValue(x => x.SelectedNotificationOptionIndex);
        return pickerFilterSource;
    }

    private (IObservable<string> TextFilter, IObservable<string> TypeFilter) CreateTextFilters()
    {
        var textFilterSource = this.WhenAnyValue(
                vm => vm.FilterText,
                vm => vm.TypeFilter
            )
            .Throttle(TimeSpan.FromMilliseconds(500));
        return (textFilterSource.Select(x => x.Item1), textFilterSource.Select(x => x.Item2));
    }
}

public class NotificationFilterInputSourceFactory(INotificationFilter viewModel)
{
    public InputSource CreateInputSource(BehaviorSubject<object?> refreshSubject)
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
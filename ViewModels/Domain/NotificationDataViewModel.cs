using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Config;
using ViewModels.Interfaces;
using ViewModels.Models;
using ViewModels.Services;

namespace ViewModels.Domain;

using NotificationCollection = List<INotification>;

public interface INotificationDataViewModel
{
    string FilterText { get; set; }
    DateTime Start { get; set; }
    DateTime End { get; set; }
    IList<string> Types { get; }
    string TypeFilter { get; set; }
    IList<string> NotificationOptions { get; set; }
    int SelectedNotificationOptionIndex { get; set; }
    ICommand ClearCommand { get; }
    DateTime MonthDate { get; set; }
    NotificationCollection NotificationItems { get; }
    int ItemCount { get; }
    int CurrentPage { get; set; }
    int Pages { get; }
    ICommand ChangePageCommand { get; }

}


public partial class NotificationDataViewModel
{
    private string _filterText = "";

    public string FilterText
    {
        get => _filterText;
        set => this.RaiseAndSetIfChanged(ref _filterText, value);
    }

    private DateTime _start = DateTime.Now.Date;

    public DateTime Start
    {
        get => _start;
        set => this.RaiseAndSetIfChanged(ref _start, value);
    }

    private DateTime _end = DateTime.Now.Date.AddMonths(1);

    public DateTime End
    {
        get => _end;
        set => this.RaiseAndSetIfChanged(ref _end, value);
    }

    public IList<string> Types { get; }

    private string _typeFilter = "";

    public string TypeFilter
    {
        get => _typeFilter;
        set => this.RaiseAndSetIfChanged(ref _typeFilter, value);
    }


    private IList<string> _notificationOptions = ["None", "True", "False"];


    public IList<string> NotificationOptions
    {
        get => _notificationOptions;
        set => this.RaiseAndSetIfChanged(ref _notificationOptions, value);
    }

    private int _selectedNotificationOptionIndex;

    public int SelectedNotificationOptionIndex
    {
        get => _selectedNotificationOptionIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedNotificationOptionIndex, value);
    }

    public ICommand ClearCommand { get; }

    private DateTime _monthDate = DateTime.Now.Date;

    public DateTime MonthDate
    {
        get => _monthDate;
        set => this.RaiseAndSetIfChanged(ref _monthDate, value);
    }

    private readonly ObservableAsPropertyHelper<NotificationCollection> _notificationItemsHelper;

    public NotificationCollection NotificationItems => _notificationItemsHelper.Value;

    private readonly ObservableAsPropertyHelper<int> _itemCountHelper;
    public int ItemCount => _itemCountHelper.Value;
    public int CurrentPage { get; set; }
    public int Pages { get; set; }
    public ICommand ChangePageCommand { get; set; }
}


[Inject]
public partial class NotificationDataViewModel : ReactiveObject, IRefresh, INotificationDataViewModel
{


    private readonly ILogger<NotificationDataViewModel> _logger;


    public NotificationDataViewModel(
        NotificationDataStreamFactory notificationDataStreamFactory, NotificationTypes types,
        ILogger<NotificationDataViewModel> logger)
    {
        _logger = logger;
        Types = types.Value;
        ClearCommand = ReactiveCommand.Create(() =>
        {
            _logger.LogDebug("Clearing filters");
            var today = DateTime.Today.Date;
            Start = new DateTime(today.Year, today.Month, today.Day);
            End = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
            FilterText = "";
            TypeFilter = "";
            SelectedNotificationOptionIndex = 0;
        });




        var inputSource = new InputSource
        {
            RefreshSource = _refreshSubject,
            DateFilterSource = CreateDateFilterSource(),
            TextFilterSource = CreateTextFilterSource(),
            PickerFilterSource = CreatePickerFilterSource()
        };

        var dataStream = notificationDataStreamFactory.CreateDataStream(inputSource);

        _itemCountHelper = dataStream
            .Select(x => x.Count)
            .Do(x => _logger.LogDebug("Notification count {Count}", x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, vm => vm.ItemCount);

        _notificationItemsHelper = dataStream
            .Do(x => _logger.LogDebug("Notification items {Items}", x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, vm => vm.NotificationItems);
    }

    private IObservable<DateTimeRange> CreateDateFilterSource()
    {
        var dateFilterSource = this
            .WhenAnyValue(
                vm => vm.Start,
                vm => vm.End,
                (start, end) => new DateTimeRange { Start = start, End = end }
            )
            .Do(x => _logger.LogDebug("Date range {Start} {End}", x.Start, x.End));
        return dateFilterSource;
    }

    private IObservable<int> CreatePickerFilterSource()
    {
        var pickerFilterSource = this.WhenAnyValue(x => x.SelectedNotificationOptionIndex)
            .Do(x => _logger.LogDebug("Picker index {Index}", x));
        return pickerFilterSource;
    }

    private IObservable<TextFilterSource> CreateTextFilterSource()
    {
        var textFilterSource = this.WhenAnyValue(
                vm => vm.FilterText,
                vm => vm.TypeFilter)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Select(x => new TextFilterSource(x.Item1, x.Item2))
            .Do(x => _logger.LogDebug("Filters: {Data}", x));
        return textFilterSource;
    }

    private readonly BehaviorSubject<object?> _refreshSubject = new(new object());

    public Task RefreshAsync()
    {
        _logger.LogDebug("Refreshing notification data");
        _refreshSubject.OnNext(new object());
        return Task.CompletedTask;
    }
}
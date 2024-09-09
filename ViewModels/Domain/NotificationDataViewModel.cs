using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Lib.Utils;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Config;
using ViewModels.Interfaces;
using ViewModels.Models;
using ViewModels.Services;

namespace ViewModels.Domain;

using NotificationCollection = List<INotification>;

[Inject]
public class NotificationDataViewModel : ReactiveObject, IRefresh
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


    private List<string> _notificationOptions = ["None", "True", "False"];


    public List<string> NotificationOptions
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

    private static ParallelQuery<INotification> ApplyNotificationFilter(
        ParallelQuery<INotification> results,
        int notificationSelectedIndex
    ) =>
        notificationSelectedIndex switch
        {
            < 1 => results,
            1 => results.Where(item => item.ShouldNotify),
            > 1 => results.Where(item => !item.ShouldNotify)
        };

    public ReactiveCommand<Unit, Unit> ClearCommand { get; }

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

    private readonly ILogger<NotificationDataViewModel> _logger;


    public NotificationDataViewModel(
        DataStreamFactory dataStreamFactory, NotificationTypes types, ILogger<NotificationDataViewModel> logger)
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


        var refreshSource = _refreshSubject;

        var textFilterSource = CreateTextFilterSource();

        var pickerFilterSource = CreatePickerFilterSource();

        var dateFilterSource = CreateDateFilterSource();

        var dataStream = dataStreamFactory.CreateDataStream(new InputSource(dateFilterSource, textFilterSource,
            pickerFilterSource, refreshSource));

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

    private IObservable<(string, string)> CreateTextFilterSource()
    {
        var textFilterSource = this.WhenAnyValue(
                vm => vm.FilterText,
                vm => vm.TypeFilter)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Do(x => _logger.LogDebug("Filter text {FilterText} {TypeFilter}", x.Item1, x.Item2));
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
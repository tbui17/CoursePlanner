using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
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
    NotificationCollection NotificationItems { get; }
    int ItemCount { get; }
    int CurrentPage { get; set; }
    int Pages { get; }
    ICommand ChangePageCommand { get; }
    ICommand ClearCommand { get; }
}


partial class NotificationDataViewModel
{
    [Reactive]
    public string FilterText { get; set; }

    [Reactive]
    public DateTime Start { get; set; }

    [Reactive]
    public DateTime End { get; set; }

    [ObservableAsProperty]
    public IList<string> Types { get; }

    [Reactive]
    public string TypeFilter { get; set; } = "";

    [Reactive]
    public IList<string> NotificationOptions { get; set; }

    [Reactive]
    public int SelectedNotificationOptionIndex { get; set; }



    [ObservableAsProperty]
    public NotificationCollection NotificationItems { get; }

    [ObservableAsProperty]
    public int ItemCount { get; }

    [Reactive]
    public int CurrentPage { get; set; }

    [ObservableAsProperty]
    public int Pages { get; }


    public ICommand ChangePageCommand { get; set; }

    public ICommand ClearCommand { get; }
}


[Inject]
public partial class NotificationDataViewModel : ReactiveObject, IRefresh, INotificationDataViewModel
{


    private readonly ILogger<NotificationDataViewModel> _logger;


    public NotificationDataViewModel(
        NotificationDataStreamFactory notificationDataStreamFactory, NotificationTypes types,
        ILogger<NotificationDataViewModel> logger)
    {
        var now = DateTime.Now.Date;
        FilterText = "";
        Start = now;
        End = now;
        NotificationItems = [];
        ChangePageCommand = ReactiveCommand.Create<int>(page => CurrentPage = page);
        NotificationOptions = new List<string> { "None", "True", "False" };
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

        dataStream
            .Select(x => x.Count)
            .Do(x => _logger.LogDebug("Notification count {Count}", x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, vm => vm.ItemCount);

        dataStream
            .Do(x => _logger.LogDebug("Notification items {Items}", x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToPropertyEx(this, vm => vm.NotificationItems);
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
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
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
    bool Busy { get; }
    string TypeFilter { get; set; }
    IList<string> NotificationOptions { get; set; }
    int SelectedNotificationOptionIndex { get; set; }

    int CurrentPage { get; set; }
    IList<string>? Types { get; }
    int ItemCount { get; }
    int Pages { get; }
    NotificationCollection? NotificationItems { get; }
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

    public bool Busy { get; private set; }

    [Reactive]
    public string TypeFilter { get; set; } = "";

    [Reactive]
    public IList<string> NotificationOptions { get; set; }

    [Reactive]
    public int SelectedNotificationOptionIndex { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [ObservableAsProperty]
    public int Pages { get; }

    [ObservableAsProperty]
    public IList<string> Types { get; }

    [ObservableAsProperty]
    public NotificationCollection? NotificationItems { get; }

    [ObservableAsProperty]
    public int ItemCount { get; }


    public ICommand ChangePageCommand { get; }

    public ICommand ClearCommand { get; }
}

[Inject]
public partial class NotificationDataViewModel : ReactiveObject, IRefresh, INotificationDataViewModel
{
    private readonly ILogger<NotificationDataViewModel> _logger;


    public NotificationDataViewModel(
        NotificationDataStreamFactory notificationDataStreamFactory,
        ILogger<NotificationDataViewModel> logger
    )
    {
        var now = DateTime.Now.Date;
        FilterText = "";
        Start = now;
        End = now;
        ChangePageCommand = ReactiveCommand.Create<int>(page => { CurrentPage = Math.Min(page, Pages); });
        NotificationOptions = new List<string> { "None", "True", "False" };
        _logger = logger;
        Types = new NotificationTypes(["Assessment", "Course"]).Value;
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

        var pageSource = this.WhenAnyValue(x => x.CurrentPage);


        var dataStream = CreateInputSource()
            .Thru(notificationDataStreamFactory.Create)
            .Thru(x => new InputSourceWithCurrentPage { Data = x, CurrentPage = pageSource })
            .Thru(notificationDataStreamFactory.Create);


        dataStream
            .Select(x => x.Data)
            .Do(x => _logger.LogDebug("Notification items {Items}", x))
            .Thru(x => ToPropertyEx(x, vm => vm.NotificationItems));

        dataStream
            .Select(x => x.Data.Count)
            .Thru(x => ToPropertyEx(x, vm => vm.ItemCount));

        dataStream
            .Select(x => x.PageCount)
            .Thru(x => ToPropertyEx(x, vm => vm.Pages));
    }


    private InputSource CreateInputSource()
    {
        var inputSource = new InputSource
        {
            Refresh = _refreshSubject,
            DateFilter = CreateDateFilterSource(),
            TextFilter = CreateTextFilterSource(),
            PickerFilter = CreatePickerFilterSource()
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

    private ObservableAsPropertyHelper<TRet> ToPropertyEx<TRet>(
        IObservable<TRet> item,
        Expression<Func<NotificationDataViewModel, TRet>> property)
    {
        return item.ToPropertyEx(this, property, scheduler: RxApp.MainThreadScheduler);
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
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
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain;

using NotificationCollection = List<INotification>;

public enum ShouldNotifyIndex
{
    None,
    True,
    False
}

public interface INotificationFilter : IDateTimeRange
{
    string FilterText { get; set; }
    string TypeFilter { get; set; }
    IList<string> NotificationOptions { get; set; }
    ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }
    int CurrentPage { get; set; }
    int PageSize { get; set; }
}

public interface INotificationDataViewModel : INotificationFilter
{
    IList<string>? Types { get; }

    // bool Busy { get; }
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

    // public bool Busy { get; private set; }

    [Reactive]
    public string TypeFilter { get; set; } = "";

    [Reactive]
    public IList<string> NotificationOptions { get; set; }

    [Reactive]
    public ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int PageSize { get; set; }

    [ObservableAsProperty]
    public IPageResult? PageResult { get; set; }


    [ObservableAsProperty]
    public IList<string> Types { get; }

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
        PageSize = 10;
        FilterText = "";
        Start = now;
        End = now;

        ChangePageCommand = ReactiveCommand.Create<int>(page =>
            {
                if (GetPage(PageResult) is not { } p)
                {
                    return;
                }
                CurrentPage = Math.Min(page, p);
            },
            canExecute: this.WhenAnyValue(x => x.PageResult).Select(x => GetPage(x) is not null)
        );
        NotificationOptions = new List<string> { "None", "True", "False" };
        _logger = logger;
        Types = new NotificationTypes(["Objective Assessment", "Performance Assessment", "Course"]).Value;
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


        var pageResult = notificationDataStreamFactory.CreatePageDataStream(CreateInputSource());
        ToPropertyEx(pageResult, x => x.PageResult);


    }

    private static int? GetPage(IPageResult? pageResult)
    {
        if (pageResult is not { CurrentPage: var p and > 1 })
        {
            return null;
        }

        return p;
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
            )
            .Do(x => _logger.LogDebug("Date range {Start} {End}", x.Start, x.End));
        return dateFilterSource;
    }

    private IObservable<ShouldNotifyIndex> CreatePickerFilterSource()
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

    private (IObservable<string> TextFilter, IObservable<string> TypeFilter) CreateTextFilters()
    {
        var textFilterSource = this.WhenAnyValue(
                vm => vm.FilterText,
                vm => vm.TypeFilter)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Do(x => _logger.LogDebug("Filters: {Data}", x));
        return (textFilterSource.Select(x => x.Item1), textFilterSource.Select(x => x.Item2));
    }

    private readonly BehaviorSubject<object?> _refreshSubject = new(new object());


    public Task RefreshAsync()
    {
        _logger.LogDebug("Refreshing notification data");
        _refreshSubject.OnNext(new object());
        return Task.CompletedTask;
    }
}
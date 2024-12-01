using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Lib;
using Lib.Attributes;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Interfaces;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain.NotificationDataViewModel;

partial class NotificationDataViewModel
{
    private readonly Subject<DateTime> _endDateOverride = new();

    private readonly ObservableAsPropertyHelper<IPageResult> _pageResult;

    private readonly Subject<DateTime> _startDateOverride = new();


    public IObservable<DateTime> StartDateObservable { get; }
    public IObservable<DateTime> EndDateObservable { get; }
    public IPageResult PageResult => _pageResult.Value;

    public IList<string> Types { get; }

    public ICommand ChangePageCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }

    [Reactive]
    public string FilterText { get; set; }

    [Reactive]
    public DateTime Start { get; internal set; }

    [Reactive]
    public DateTime End { get; internal set; }

    [Reactive]
    public string TypeFilter { get; set; }


    public IList<string> NotificationOptions { get; } = new List<string>
    {
        "All", "Notifications Enabled", "Notifications Disabled"
    };

    [Reactive]
    public ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int PageSize { get; set; }
}

public interface INotificationDataViewModel
{
    string FilterText { get; set; }
    DateTime Start { get; }
    DateTime End { get; }
    string TypeFilter { get; set; }
    IList<string> NotificationOptions { get; }
    ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }
    int CurrentPage { get; set; }
    int PageSize { get; set; }
    IObservable<DateTime> StartDateObservable { get; }
    IObservable<DateTime> EndDateObservable { get; }
    IPageResult PageResult { get; }
    IList<string> Types { get; }
    ICommand ChangePageCommand { get; }
    ICommand ClearCommand { get; }
    ICommand NextCommand { get; }
    ICommand PreviousCommand { get; }
    void ChangeEndDate(DateChangedEventArgs args);
    void ChangeStartDate(DateChangedEventArgs dateChangedEventArgs);
    Task RefreshAsync();
}

[Inject]
public partial class NotificationDataViewModel : ReactiveObject, INotificationFilter, IRefresh,
    INotificationDataViewModel
{
    private readonly ILogger<NotificationDataViewModel> _logger;
    private readonly INotificationFilterService _notificationFilterService;


    public NotificationDataViewModel(
        INotificationFilterService notificationFilterService,
        ILogger<NotificationDataViewModel> logger,
        INotificationDataViewModelDefaultsProvider defaultsProvider,
        AutocompleteService autocompleteService
    )
    {
        #region init

        _notificationFilterService = notificationFilterService;
        _logger = logger;

        CurrentPage = 1;
        PageSize = defaultsProvider.PageSize;

        FilterText = "";
        TypeFilter = "";

        var dateRange = defaultsProvider.DateRange;
        Start = dateRange.Start;
        End = dateRange.End;

        Types = autocompleteService.GetNotificationTypes();

        #endregion


        #region properties

        _pageResult = notificationFilterService
            .Connect(this)
            .Do(x => _logger.LogInformation("Page result {PageResult}", x))
            .ToProperty(this, x => x.PageResult, new EmptyPageResult(), scheduler: RxApp.MainThreadScheduler);


        notificationFilterService.CurrentPageOverridden.Subscribe(x => CurrentPage = x);

        StartDateObservable = this.WhenAnyValue(x => x.Start).Merge(_startDateOverride);
        EndDateObservable = this.WhenAnyValue(x => x.End).Merge(_endDateOverride);

        #endregion


        #region commands

        ChangePageCommand = ReactiveCommand.Create<int>(
            ChangePage,
            this.WhenAnyValue(x => x.PageResult).Select(x => x.PageCount > 1),
            RxApp.MainThreadScheduler
        );

        NextCommand = ReactiveCommand.Create(
            () => ChangePage(CurrentPage + 1),
            this.WhenAnyValue(x => x.PageResult).Select(x => x.HasNext),
            RxApp.MainThreadScheduler
        );
        PreviousCommand = ReactiveCommand.Create(
            () => ChangePage(CurrentPage - 1),
            this.WhenAnyValue(x => x.PageResult).Select(x => x.HasPrevious),
            RxApp.MainThreadScheduler
        );

        ClearCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogDebug("Clearing filters");
                var dateRange2 = defaultsProvider.DateRange;
                Start = dateRange2.Start;
                End = dateRange2.End;
                FilterText = "";
                TypeFilter = "";
                SelectedNotificationOptionIndex = 0;
            }
        );

        #endregion
    }

    public void ChangeEndDate(DateChangedEventArgs args)
    {
        if (args.OldDate == End && args.NewDate < Start)
        {
            _endDateOverride.OnNext(Start);
            return;
        }

        ChangeEndDate(args.NewDate);
    }

    public void ChangeStartDate(DateChangedEventArgs dateChangedEventArgs)
    {
        if (dateChangedEventArgs.OldDate == Start && dateChangedEventArgs.NewDate > End)
        {
            _startDateOverride.OnNext(End);
            return;
        }

        ChangeStartDate(dateChangedEventArgs.NewDate);
    }

    public Task RefreshAsync()
    {
        using var _ = _logger.MethodScope();
        Refresh();
        return Task.CompletedTask;
    }

    private void ChangePage(int page)
    {
        _logger.LogDebug("Received {Page}", page);
        if (PageResult is not PageResult { PageCount: var max })
        {
            return;
        }


        var newPage = Math.Clamp(page, 1, max);
        _logger.LogDebug("Changing page to {NewPage}", newPage);
        CurrentPage = newPage;
    }


    internal void ChangeEndDate(DateTime newEnd)
    {
        if (Start > newEnd)
        {
            End = Start;
            return;
        }

        End = newEnd;
    }

    internal void ChangeStartDate(DateTime newStart)
    {
        using var _ = _logger.MethodScope();
        if (newStart > End)
        {
            Start = End;
            return;
        }

        Start = newStart;
    }

    private void Refresh()
    {
        _logger.LogDebug("Refreshing notification data");
        _notificationFilterService.Refresh();
    }
}
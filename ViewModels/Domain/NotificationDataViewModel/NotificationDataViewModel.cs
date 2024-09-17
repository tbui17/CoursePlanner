using System.Collections.ObjectModel;
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

    private readonly Subject<DateTime> _startDateOverride = new();
    private readonly Subject<DateTime> _endDateOverride = new();


    public IObservable<DateTime> StartDateObservable { get; }
    public IObservable<DateTime> EndDateObservable { get; }

    [Reactive]
    public IPageResult PageResult { get; private set; }

    public ReadOnlyObservableCollection<string> Types { get; }

    public ICommand ChangePageCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PreviousCommand { get; }
}

[Inject]
public partial class NotificationDataViewModel : ReactiveObject, INotificationFilter, IRefresh
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

        PageResult = new EmptyPageResult();

        _notificationFilterService = notificationFilterService;
        _logger = logger;

        CurrentPage = 1;
        PageSize = defaultsProvider.PageSize;

        FilterText = "";
        TypeFilter = "";

        var dateRange = defaultsProvider.DateRange;
        Start = dateRange.Start;
        End = dateRange.End;

        Types = autocompleteService.BindSubscribe();

        #endregion


        #region properties

        notificationFilterService
            .Connect(this)
            .Do(x => _logger.LogInformation("Page result {PageResult}", x))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => PageResult = x);

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

    public void ChangeEndDate(DateChangedEventArgs args)
    {
        if (args.OldDate == End && args.NewDate < Start)
        {
            _endDateOverride.OnNext(Start);
            return;
        }

        ChangeEndDate(args.NewDate);
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

    public void ChangeStartDate(DateChangedEventArgs dateChangedEventArgs)
    {
        if (dateChangedEventArgs.OldDate == Start && dateChangedEventArgs.NewDate > End)
        {
            _startDateOverride.OnNext(End);
            return;
        }

        ChangeStartDate(dateChangedEventArgs.NewDate);
    }

    private void Refresh()
    {
        _logger.LogDebug("Refreshing notification data");
        _notificationFilterService.Refresh();
    }

    public Task RefreshAsync()
    {
        using var _ = _logger.MethodScope();
        Refresh();
        return Task.CompletedTask;
    }
}
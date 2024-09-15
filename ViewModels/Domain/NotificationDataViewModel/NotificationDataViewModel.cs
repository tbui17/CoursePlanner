using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Lib.Attributes;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Interfaces;
using ViewModels.Services.NotificationDataStreamFactory;
using OneOf;

namespace ViewModels.Domain.NotificationDataViewModel;


using DateArgs = OneOf<DateTime, DateChangedEventArgs>;

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
        { "All", "Notifications Enabled", "Notifications Disabled" };

    [Reactive]
    public ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int PageSize { get; set; }

    internal readonly Subject<DateTime> _startDateOverride = new();
    internal readonly Subject<DateTime> _endDateOverride = new();


    public IObservable<DateTime> StartDateObservable { get; }
    public IObservable<DateTime> EndDateObservable { get; }

    private readonly ObservableAsPropertyHelper<IPageResult> _pageResult;
    public IPageResult PageResult => _pageResult.Value;
    public ReadOnlyObservableCollection<string> Types { get; }

    public ICommand ChangePageCommand { get; }
    public ICommand ClearCommand { get; }
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

        _pageResult = notificationFilterService.Connect(this)
            .Do(x => _logger.LogInformation("Page result {PageResult}", x))
            .ToProperty(this, x => x.PageResult, scheduler: RxApp.MainThreadScheduler);

        StartDateObservable = this.WhenAnyValue(x => x.Start).Merge(_startDateOverride);
        EndDateObservable = this.WhenAnyValue(x => x.End).Merge(_endDateOverride);

        #endregion


        #region commands

        ChangePageCommand = ReactiveCommand.Create<int>(page =>
            {
                var max = GetOrThrowMaxPage();


                var newPage = Math.Clamp(page, 1, max);
                CurrentPage = newPage;
                return;

                int GetOrThrowMaxPage()
                {
                    if (PageResult is not { PageCount: var pageCount and > 0 })
                    {
                        var err = new UnreachableException("Unexpected page result state.");
                        logger.LogError(err,
                            "Unexpected page result state. {PageResult} {Page} {CurrentPage}",
                            PageResult,
                            page,
                            CurrentPage
                        );
                        throw err;
                    }

                    return pageCount;
                }
            }
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

    public void ChangeDate(DateState snapshot)
    {
        var handler = new HandlerFactory(this).Create(snapshot);
        handler.Invoke();
    }

    internal void ChangeEndDate(DateArgs args)
    {
        args.Switch(HandleDateTime, HandleDateChangedEventArgs);

        return;

        void HandleDateTime(DateTime newEnd)
        {
            if (Start > newEnd)
            {
                End = Start;
                return;
            }

            End = newEnd;
        }

        void HandleDateChangedEventArgs(DateChangedEventArgs dargs)
        {
            if (dargs.OldDate == End && dargs.NewDate < Start)
            {
                _endDateOverride.OnNext(Start);
                return;
            }

            HandleDateTime(dargs.NewDate);
        }
    }

    internal void ChangeStartDate(DateArgs args)
    {
        args.Switch(HandleDateTime, HandleDateChangedEventArgs);
        return;

        void HandleDateTime(DateTime newStart)
        {
            if (newStart > End)
            {
                Start = End;
                return;
            }

            Start = newStart;
        }

        void HandleDateChangedEventArgs(DateChangedEventArgs dateChangedEventArgs)
        {
            if (dateChangedEventArgs.OldDate == Start && dateChangedEventArgs.NewDate > End)
            {
                _startDateOverride.OnNext(End);
                return;
            }

            HandleDateTime(dateChangedEventArgs.NewDate);
        }
    }

    private void Refresh()
    {
        _logger.LogDebug("Refreshing notification data");
        _notificationFilterService.Refresh();
    }

    public Task RefreshAsync()
    {
        Refresh();
        return Task.CompletedTask;
    }
}
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
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
    public DateTime Start { get; private set; }

    [Reactive]
    public DateTime End { get; private set; }

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


    private readonly ObservableAsPropertyHelper<IPageResult> _pageResult;
    public IPageResult PageResult => _pageResult.Value;
    public IList<string> Types { get; }
    public ICommand ChangePageCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand ChangeStartDateCommand { get; set; }
    public ICommand ChangeEndDateCommand { get; set; }
}

[Inject]
public partial class NotificationDataViewModel : ReactiveObject,INotificationFilter, IRefresh
{
    private readonly ILogger<NotificationDataViewModel> _logger;
    private readonly INotificationFilterService _notificationFilterService;


    public NotificationDataViewModel(
        INotificationFilterService notificationFilterService,
        ILogger<NotificationDataViewModel> logger,
        INotificationDataViewModelDefaultsProvider defaultsProvider
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

        Types = new List<string> { "Objective Assessment", "Performance Assessment", "Course" };

        #endregion


        #region properties

        _pageResult = notificationFilterService.Connect(this)
            .Do(x => _logger.LogInformation("Page result {PageResult}", x))
            .ToProperty(this,x => x.PageResult,scheduler:RxApp.MainThreadScheduler);
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
                var today = DateTime.Today.Date;
                Start = new DateTime(today.Year, today.Month, today.Day);
                End = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                FilterText = "";
                TypeFilter = "";
                SelectedNotificationOptionIndex = 0;
            }
        );

        ChangeStartDateCommand = ReactiveCommand.Create<DateTime>(newStart =>
            {
                if (newStart > End)
                {
                    Start = End;
                    return;
                }

                if (newStart == Start)
                {
                    Refresh();
                    return;
                }

                Start = newStart;
            }
        );

        ChangeEndDateCommand = ReactiveCommand.Create<DateTime>(newEnd =>
            {
                if (Start > newEnd)
                {
                    End = Start;
                    return;
                }

                if (newEnd == End)
                {
                    Refresh();
                    return;
                }

                End = newEnd;
            }
        );

        #endregion

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
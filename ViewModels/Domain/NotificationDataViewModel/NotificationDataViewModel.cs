using System.Collections;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Input;
using Lib.Attributes;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Interfaces;
using ViewModels.Services.NotificationDataStreamFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationDataViewModel : INotificationFilter
{
    IList Types { get; }
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

    [Reactive]
    public string TypeFilter { get; set; }

    [Reactive]
    public IList NotificationOptions { get; private init; }

    [Reactive]
    public ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int PageSize { get; set; }

    [ObservableAsProperty]
    public IPageResult PageResult { get; }


    [Reactive]
    public IList Types { get; private init; }

    public ICommand ChangePageCommand { get; }

    public ICommand ClearCommand { get; }
}

[Inject]
public partial class NotificationDataViewModel : ReactiveObject, IRefresh, INotificationDataViewModel
{
    private readonly ILogger<NotificationDataViewModel> _logger;
    private readonly INotificationFilterService _notificationFilterService;


    public NotificationDataViewModel(
        INotificationFilterService notificationFilterService,
        ILogger<NotificationDataViewModel> logger,
        IDefaultDateProvider defaultDateProvider,
        IDefaultPageProvider defaultPageProvider
    )
    {
        // init
        _notificationFilterService = notificationFilterService;
        _logger = logger;

        CurrentPage = 1;
        PageSize = defaultPageProvider.PageSize;

        FilterText = "";
        TypeFilter = "";

        var dateRange = defaultDateProvider.DateRange;
        Start = dateRange.Start;
        End = dateRange.End;

        NotificationOptions = new List<string> { "All", "Notifications Enabled", "Notifications Disabled" };
        Types = new ArrayList();

        // properties
        var pageResult = notificationFilterService.Connect(this);

        pageResult
            .Do(x => _logger.LogInformation("Page result {PageResult}", x))
            .ToPropertyEx(
                this,
                x => x.PageResult,
                scheduler: RxApp.MainThreadScheduler
            );


        // commands

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
    }


    public Task RefreshAsync()
    {
        _logger.LogDebug("Refreshing notification data");
        _notificationFilterService.Refresh();

        return Task.CompletedTask;
    }
}
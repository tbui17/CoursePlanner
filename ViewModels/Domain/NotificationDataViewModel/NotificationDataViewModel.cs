using System.Collections;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;
using Lib.Attributes;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViewModels.Config;
using ViewModels.Interfaces;
using ViewModels.Services.NotificationDataStreamFactory;

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
    public string TypeFilter { get; set; } = "";

    [Reactive]
    public IList NotificationOptions { get; private init; }

    [Reactive]
    public ShouldNotifyIndex SelectedNotificationOptionIndex { get; set; }

    [Reactive]
    public int CurrentPage { get; set; }

    [Reactive]
    public int PageSize { get; set; }

    [ObservableAsProperty]
    public IPageResult? PageResult { get; set; }


    [Reactive]
    public IList Types { get; private init; }

    public ICommand ChangePageCommand { get; }

    public ICommand ClearCommand { get; }
}

[Inject]
public partial class NotificationDataViewModel : ReactiveObject, IRefresh, INotificationDataViewModel
{
    private readonly ILogger<NotificationDataViewModel> _logger;


    public NotificationDataViewModel(
        NotificationDataStreamFactory notificationDataStreamFactory,
        ILogger<NotificationDataViewModel> logger,
        IDefaultDateProvider? defaultDateProvider,
        IDefaultPageProvider? defaultPageProvider
    )
    {
        defaultDateProvider ??= new DefaultDateProvider();
        defaultPageProvider ??= new DefaultPageProvider();
        var dateRange = defaultDateProvider.DateRange;
        // init
        _logger = logger;
        CurrentPage = 1;
        var now = dateRange.Start;
        PageSize = defaultPageProvider.PageSize;
        FilterText = "";

        Start = now;
        End = dateRange.End;

        NotificationOptions = new List<string>
            { "All", "Notifications Enabled", "Notifications Disabled" };
        Types = new NotificationTypes(["Objective Assessment", "Performance Assessment", "Course"]).Value;


        // properties
        var pageResult =
            notificationDataStreamFactory.CreatePageDataStream(
                new NotificationDataViewModelInputSourceFactory(this).CreateInputSource(_refreshSubject)
            );

        pageResult
            .StartWith(new EmptyPageResult())
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

    private readonly BehaviorSubject<object?> _refreshSubject = new(new object());


    public Task RefreshAsync()
    {
        _logger.LogDebug("Refreshing notification data");
        _refreshSubject.OnNext(new object());
        return Task.CompletedTask;
    }
}
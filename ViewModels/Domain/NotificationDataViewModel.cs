using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.NotificationService;
using Lib.Utils;
using NodaTime;
using ReactiveUI;
using ViewModels.Config;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

using NotificationCollection = List<INotification>;

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

    public NotificationDataViewModel(
        NotificationService service,
        [FromKeyedServices(nameof(TypesSource))]
        IList<string> typesSource
    )
    {
        Types = typesSource;
        ClearCommand = ReactiveCommand.Create(() =>
        {
            var today = LocalDateTime.FromDateTime(DateTime.Today.Date);
            Start = today.With(DateAdjusters.StartOfMonth).ToDateTimeUnspecified();
            End = today.With(DateAdjusters.EndOfMonth).ToDateTimeUnspecified();
            FilterText = "";
            TypeFilter = "";
            SelectedNotificationOptionIndex = 0;
        });


        var refreshSource = _refreshSubject;

        var textFilterSource = this.WhenAnyValue(
                vm => vm.FilterText,
                vm => vm.TypeFilter)
            .Throttle(TimeSpan.FromMilliseconds(500));

        var pickerFilterSource = this.WhenAnyValue(x => x.SelectedNotificationOptionIndex);

        var dateFilterSource = this
            .WhenAnyValue(
                vm => vm.Start,
                vm => vm.End,
                (start, end) => new DateTimeRange { Start = start, End = end }
            );

        var dataStream = dateFilterSource
            .ObserveOn(RxApp.TaskpoolScheduler)
            .SelectMany(service.GetNotificationsWithinDateRange)
            .CombineLatest(textFilterSource, pickerFilterSource, refreshSource)
            .Select(sources =>
            {
                var (notifications, (filterText, typeFilter), notificationSelectedIndex, _) = sources;

                return notifications
                    .AsParallel()
                    .Thru(x => ApplyNotificationFilter(x, notificationSelectedIndex))
                    .Where(item => item.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase))
                    .Where(item =>
                        item is Assessment assessment
                            ? $"{assessment.Type} Assessment".Contains(typeFilter,
                                StringComparison.CurrentCultureIgnoreCase)
                            : item.GetType().Name.Contains(typeFilter, StringComparison.CurrentCultureIgnoreCase));
            })
            .Select(x => x.ToList())
            .LoggedCatch(this, Observable.Return(new NotificationCollection()));

        _itemCountHelper = dataStream
            .Select(x => x.Count)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, vm => vm.ItemCount);

        _notificationItemsHelper = dataStream
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, vm => vm.NotificationItems);
    }

    private readonly BehaviorSubject<object?> _refreshSubject = new(new object());

    public Task RefreshAsync()
    {
        _refreshSubject.OnNext(new object());
        return Task.CompletedTask;
    }
}
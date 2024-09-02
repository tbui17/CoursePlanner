using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using ReactiveUI;
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

    public NotificationDataViewModel(NotificationService service)
    {
        var refreshSource = _refreshSubject;
        var filterSource = this.WhenAnyValue(vm => vm.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(500));

        var monthSource = this.WhenAnyValue(vm => vm.MonthDate)
            .SelectMany(service.GetNotificationsForMonth);


        var dataStream = monthSource
            .ObserveOn(RxApp.TaskpoolScheduler)
            .CombineLatest(filterSource, refreshSource)
            .Select(x =>
            {
                var (notifications, filterText, _) = x;
                return notifications
                    .AsParallel()
                    .Where(item => item.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase));
            })
            .Select(x => x.ToList());

        _itemCountHelper = dataStream
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(x => x.Count)
            .ToProperty(this, vm => vm.ItemCount);

        _notificationItemsHelper = dataStream
            .ObserveOn(RxApp.MainThreadScheduler)
            .LoggedCatch(this, Observable.Return(new NotificationCollection()))
            .ToProperty(this, vm => vm.NotificationItems);
    }

    private readonly BehaviorSubject<object?> _refreshSubject = new(new object());

    public Task RefreshAsync()
    {
        _refreshSubject.OnNext(new object());
        return Task.CompletedTask;
    }
}
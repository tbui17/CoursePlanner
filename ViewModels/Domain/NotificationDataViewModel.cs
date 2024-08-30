using System.ComponentModel;
using System.Reactive.Linq;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using ReactiveUI;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

using NotificationCollection = List<INotification>;

public class NotificationDataViewModel : ReactiveObject, IRefresh0
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

    private readonly ObservableAsPropertyHelper<int> _totalItemsHelper;
    public int TotalItems => _totalItemsHelper.Value;

    private readonly ObservableAsPropertyHelper<int> _itemCountHelper;
    public int ItemCount => _itemCountHelper.Value;

    public NotificationDataViewModel(NotificationService service)
    {
        var refreshStream = MessageBus.Current.Listen<RefreshEventArg>();
        var filterStream = this.WhenAnyValue(vm => vm.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(500));

        var monthStream = this.WhenAnyValue(vm => vm.MonthDate)
            .SelectMany(service.GetNotificationsForMonth);

        _totalItemsHelper = refreshStream
            .SelectMany(_ => service.GetTotalItems())
            .ToProperty(this, vm => vm.TotalItems);



        var dataStream = monthStream
            .ObserveOn(RxApp.TaskpoolScheduler)
            .CombineLatest(filterStream)
            .Select(x =>
            {
                var (notifications, filterText) = x;
                return notifications
                    .AsParallel()
                    .Where(item => item.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase));
            })
            .Select(x => x.ToList());

        _itemCountHelper = dataStream
            .ObserveOn(RxApp.MainThreadScheduler)
            .Select(x => x.Count)
            .ToProperty(this, vm => vm.TotalItems);

        _notificationItemsHelper = dataStream
            .ObserveOn(RxApp.MainThreadScheduler)
            .LoggedCatch(this, Observable.Return(new NotificationCollection()))
            .ToProperty(this, vm => vm.NotificationItems);
    }

    public Task RefreshAsync()
    {
        MessageBus.Current.SendMessage(new RefreshEventArg());
        return Task.CompletedTask;
    }
}

file class RefreshEventArg : EventArgs;
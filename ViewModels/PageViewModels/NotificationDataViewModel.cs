using System.Reactive.Linq;
using Lib.Interfaces;
using Lib.Services;
using Lib.Services.NotificationService;
using ReactiveUI;

namespace ViewModels.PageViewModels;

using NotificationCollection = List<INotification>;

public class NotificationDataViewModel : ReactiveObject
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

    public NotificationDataViewModel(NotificationService service)
    {
        var filterStream = this.WhenAnyValue(vm => vm.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(500));

        var monthStream = this.WhenAnyValue(vm => vm.MonthDate)
            .SelectMany(service.GetNotificationsForMonth);

        var dataStream = monthStream
            .ObserveOn(RxApp.TaskpoolScheduler)
            .CombineLatest(filterStream)
            .Select(pair =>
            {
                var (notifications, filterText) = pair;
                return notifications
                    .AsParallel()
                    .Where(item => item.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase));
            })
            .Select(x => x.ToList());

        _notificationItemsHelper = dataStream
            .ObserveOn(RxApp.MainThreadScheduler)
            .LoggedCatch(this, Observable.Return(new NotificationCollection()))
            .ToProperty(this, vm => vm.NotificationItems);
    }
}
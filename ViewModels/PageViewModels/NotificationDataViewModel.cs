using System.Collections.Immutable;
using System.Reactive.Linq;
using Lib.Interfaces;
using Lib.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels.PageViewModels;

using NotificationCollection = List<INotification>;

public partial class NotificationDataViewModel : ReactiveObject
{
    [Reactive]
    private string _filterText = "";

    [Reactive]
    private DateTime _monthDate = DateTime.Now.Date;

    [ObservableAsProperty]
    // ReSharper disable once NotAccessedField.Local
    private NotificationCollection _notificationItems = [];

    public NotificationDataViewModel(NotificationService service)
    {
        var filterStream = this.WhenAnyValue(vm => vm.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Select(text => text.Trim().ToLowerInvariant());

        var monthStream = this.WhenAnyValue(vm => vm.MonthDate)
            .SelectMany(service.GetNotificationsForMonth);

        _notificationItemsHelper = monthStream
            .CombineLatest(filterStream)
            .Select(pair =>
            {
                var (notifications, filterText) = pair;
                return notifications.Where(item => item.Name.Contains(filterText));
            })
            .Select(x => x.ToList())
            .LoggedCatch(this,Observable.Return(new NotificationCollection()))
            .ToProperty(this, vm => vm.NotificationItems);
    }
}
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CommunityToolkit.Maui.Core.Extensions;
using Lib.Interfaces;
using Lib.Services;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace ViewModels.PageViewModels;

using NotificationCollection = ObservableCollection<INotification>;

public partial class NotificationDataViewModel : ReactiveObject
{
    private static DateTime DefaultDate => DateTime.Now.Date;

    private readonly NotificationService _service;

    [Reactive]
    private string _filterText = "";

    [Reactive]
    private DateTime _monthDate = DefaultDate;

    [ObservableAsProperty]
    private NotificationCollection _notificationItems = [];

    public NotificationDataViewModel(NotificationService service)
    {
        _service = service;
        InitializeCommands();

        var filterS = this.WhenAnyValue(x => x.FilterText)
            .Select(x => x.Trim().ToLowerInvariant());

        var monthS = this.WhenAnyValue(x => x.MonthDate)
            .SelectMany(x => _service.GetNotificationsForMonth(x));

        _notificationItemsHelper = monthS
            .CombineLatest(filterS)
            // .ForkJoin(filterS, (x, y) => (Notifications: x, Filter: y))
            .Select(p => p.First.Where(x => x.Name.Contains(p.Second)))
            .Select(x => x.ToObservableCollection())
            .ToProperty(this, x => x.NotificationItems);
    }

    [ReactiveCommand]
    private async Task LoadNotifications()
    {
        var notifications = await _service.GetNotificationsForMonth(MonthDate);
        notifications.ToObservableCollection();
    }
}
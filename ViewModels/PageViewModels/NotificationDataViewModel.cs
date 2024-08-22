using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Interfaces;
using Lib.Services;

namespace ViewModels.PageViewModels;


public partial class NotificationDataViewModel : ObservableObject
{
    private ObservableCollection<INotification> _notificationItems = [];

    public ObservableCollection<INotification> NotificationItems
    {
        get => _notificationItems.Where(x => x.Name.Contains(FilterText,StringComparison.InvariantCultureIgnoreCase)).ToObservableCollection();
        set => SetProperty(ref _notificationItems, value);
    }

    [ObservableProperty]
    private string _filterText = "";


    [ObservableProperty]
    private DateTime _monthDate = DefaultDate;

    private readonly ILocalDbCtxFactory _factory;
    private readonly NotificationService _service;


    public NotificationDataViewModel(ILocalDbCtxFactory factory, NotificationService service)
    {
        _factory = factory;
        _service = service;
        PropertyChanged += OnPropertyChangedEventHandler;
    }

    private async void OnPropertyChangedEventHandler(object? _, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(MonthDate):
                await LoadNotificationsAsync();
                return;
            case nameof(FilterText):
                OnPropertyChanged(nameof(NotificationItems));
                return;
        }
    }

    private static DateTime DefaultDate => DateTime.Now.Date;

    private async Task LoadNotificationsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        var notifications = await _service.GetNotificationsForMonth(MonthDate);
        NotificationItems = notifications.ToObservableCollection();
    }
}
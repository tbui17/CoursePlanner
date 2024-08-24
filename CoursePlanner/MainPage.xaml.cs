
using CoursePlanner.Views;
using Plugin.LocalNotification;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    private readonly LocalNotificationService _localNotificationService;
    public MainViewModel Model { get; set; }

    public void SetView(IView view)
    {
        MainLayout.Children.Clear();
        MainLayout.Children.Add(view);
    }

    public MainPage(MainViewModel model, LoginView view, LocalNotificationService localNotificationService)
    {
        _localNotificationService = localNotificationService;
        Model = model;
        InitializeComponent();
        BindingContext = this;

        MainLayout.Children.Add(view);

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.Init();
        await _localNotificationService.RequestNotificationPermissions();
    }
}
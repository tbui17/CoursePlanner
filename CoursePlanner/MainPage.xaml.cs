
using CoursePlanner.Views;
using Plugin.LocalNotification;
using ViewModels.PageViewModels;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    public MainViewModel Model { get; set; }

    public void SetView(IView view)
    {
        MainLayout.Children.Clear();
        MainLayout.Children.Add(view);
    }

    public MainPage(MainViewModel model, LoginView view)
    {
        Model = model;
        InitializeComponent();
        BindingContext = this;

        MainLayout.Children.Add(view);

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.Init();

        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
    }
}
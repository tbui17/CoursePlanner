using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    public MainViewModel Model { get; set; }

    public MainPage(MainViewModel model)
    {
        Model = model;
        model.ShowWindowRequested += OnShowWindow;
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.GetTermsAsync();
    }

    private void OnShowWindow(MainViewModel mainViewModel)
    {
        DisplayAlert("abc", "abc", "abc");
    }
}
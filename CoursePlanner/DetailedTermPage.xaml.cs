using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class DetailedTermPage
{
    public DetailedTermViewModel Model { get; set; }

    public DetailedTermPage(DetailedTermViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;
    }
    
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}
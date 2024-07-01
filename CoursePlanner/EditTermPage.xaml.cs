using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class EditTermPage
{
    public EditTermViewModel Model { get; set; }

    public EditTermPage(EditTermViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}
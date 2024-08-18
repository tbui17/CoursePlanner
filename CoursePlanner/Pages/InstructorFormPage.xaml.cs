using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class InstructorFormPage : ContentPage
{
    public InstructorFormPage(InstructorFormViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }

    public InstructorFormViewModel Model { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}
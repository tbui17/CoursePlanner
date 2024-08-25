using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class InstructorFormPage : ContentPage
{
    public InstructorFormPage(IInstructorFormViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }

    public IInstructorFormViewModel Model { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}
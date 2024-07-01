
using CoursePlanner.ViewModels;


namespace CoursePlanner;

public partial class EditCoursePage : ContentPage
{
    public EditCourseViewModel Model { get; set; }

    public EditCoursePage(EditCourseViewModel model)
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
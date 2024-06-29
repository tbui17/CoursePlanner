using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class DetailedCoursePage
{
    public DetailedCourseViewModel Model { get; }

    public DetailedCoursePage(DetailedCourseViewModel model)
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
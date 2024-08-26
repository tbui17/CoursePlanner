using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class EditCoursePage : IRefreshableView<EditCourseViewModel>
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
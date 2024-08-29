using ViewModels.Domain;
using ViewModels.Interfaces;
using ViewModels.Domain;

namespace CoursePlanner.Pages;

public partial class DetailedCoursePage : IRefreshableView<DetailedCourseViewModel>
{
    public DetailedCourseViewModel Model { get; }

    public DetailedCoursePage(DetailedCourseViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;
    }
}
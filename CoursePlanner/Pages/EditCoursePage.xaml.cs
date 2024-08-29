using ViewModels.Domain;
using ViewModels.Interfaces;
using ViewModels.Domain;

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
}
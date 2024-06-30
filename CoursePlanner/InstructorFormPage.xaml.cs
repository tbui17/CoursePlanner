using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class InstructorFormPage : ContentPage
{
    public InstructorFormPage(InstructorFormViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = Model;
        HideSoftInputOnTapped = true;
    }

    public InstructorFormViewModel Model { get; set; }
}
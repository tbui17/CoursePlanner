using ViewModels.Domain;
using ViewModels.Interfaces;
using ViewModels.Domain;

namespace CoursePlanner.Pages;

public partial class InstructorFormPage : IRefreshableView<IInstructorFormViewModel>
{
    public InstructorFormPage(IInstructorFormViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }

    public IInstructorFormViewModel Model { get; set; }
}
using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class EditAssessmentPage : IRefreshableView<IEditAssessmentViewModel>
{
    public EditAssessmentPage(IEditAssessmentViewModel model)
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

    public IEditAssessmentViewModel Model { get; set; }
}
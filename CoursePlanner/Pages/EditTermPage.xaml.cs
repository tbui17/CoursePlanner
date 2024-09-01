

using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class EditTermPage : IRefreshableView<EditTermViewModel>
{
    public EditTermViewModel Model { get; set; }

    public EditTermPage(EditTermViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }
}
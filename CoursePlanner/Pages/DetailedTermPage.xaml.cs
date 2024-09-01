using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class DetailedTermPage : IRefreshableView<DetailedTermViewModel>
{
    public DetailedTermViewModel Model { get; set; }

    public DetailedTermPage(DetailedTermViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;
    }
}
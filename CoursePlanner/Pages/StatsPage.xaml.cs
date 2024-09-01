using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class StatsPage : IRefreshableView<StatsViewModel>
{
    public StatsViewModel Model { get; set; }

    public StatsPage(StatsViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = Model;
        HideSoftInputOnTapped = true;
    }


}
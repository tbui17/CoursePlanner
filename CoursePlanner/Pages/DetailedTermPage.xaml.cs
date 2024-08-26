using ViewModels.Interfaces;
using ViewModels.PageViewModels;

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


    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}
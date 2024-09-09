
using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class SettingsPage : ContentPage, IRefreshableView<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;
    }

    public SettingsViewModel Model { get; }
}
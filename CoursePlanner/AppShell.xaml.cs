using ViewModels.PageViewModels;

namespace CoursePlanner;

public partial class AppShell : Shell
{

    public AppShell(AppShellViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;
    }

    public AppShellViewModel Model { get; set; }
}
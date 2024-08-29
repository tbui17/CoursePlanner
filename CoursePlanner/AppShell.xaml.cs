using ViewModels.Domain;
using ViewModels.Domain;

namespace CoursePlanner;

public partial class AppShell
{

    public AppShell(AppShellViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;
    }

    public AppShellViewModel Model { get; set; }
}
using ViewModels.Domain;
using ViewModels.Interfaces;
using ViewModels.Domain;

namespace CoursePlanner.Views;

public partial class LoginView : IVmView<LoginViewModel>
{
    public LoginView(LoginViewModel model)
    {
        Model = model;
        BindingContext = model;
        InitializeComponent();
    }

    public LoginViewModel Model { get; set; }
}
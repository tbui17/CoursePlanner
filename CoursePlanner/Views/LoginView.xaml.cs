using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Views;

public partial class LoginView : IRefreshableView0<LoginViewModel>
{
    public LoginView(LoginViewModel model)
    {
        Model = model;
        BindingContext = model;
        InitializeComponent();
    }

    public LoginViewModel Model { get; set; }
}
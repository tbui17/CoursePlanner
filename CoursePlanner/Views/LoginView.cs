using CommunityToolkit.Maui.Markup;
using CoursePlanner.Controls;
using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Views;

public class LoginView : ContentView, IPageModel<LoginViewModel>
{
    public LoginView(LoginViewModel model)
    {
        Model = model;
        BindingContext = model;
        InitializeComponent();
    }

    public LoginViewModel Model { get; set; }

    private void InitializeComponent()
    {
        var grid = new AutoGrid
        {
            new Label().Text("Username:"),
            new Entry().Bind(Entry.TextProperty, nameof(Model.Username)),
            new Label().Text("Password:"),
            new Entry
            {
                IsPassword = true
            }.Bind(Entry.TextProperty, nameof(Model.Password)),
        };
        Content = new VerticalStackLayout
        {
            grid,
            new Button
            {
                Text = "Login",
                Command = Model.LoginCommand
            }
        };
    }
}
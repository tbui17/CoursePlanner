using ViewModels;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    int count = 0;
    public MainViewModel Model { get; set; }

    public MainPage(MainViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = this;
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
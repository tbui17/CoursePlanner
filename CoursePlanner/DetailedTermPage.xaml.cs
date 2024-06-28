using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class DetailedTermPage : ContentPage
{
    
    public DetailedTermViewModel Model { get; set; }

    public DetailedTermPage(DetailedTermViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = this;
        

    }

  
}
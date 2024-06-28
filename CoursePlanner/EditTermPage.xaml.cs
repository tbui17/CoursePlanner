using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class EditTermPage : ContentPage
{
	public EditTermViewModel Model { get; set; }
	
	public EditTermPage(EditTermViewModel model)
	{
		Model = model;
		InitializeComponent();
		BindingContext = model;
		
	}
}
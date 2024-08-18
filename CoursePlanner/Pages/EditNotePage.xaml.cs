using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class EditNotePage : ContentPage
{
    public EditNotePage(EditNoteViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }

    public EditNoteViewModel Model { get; set; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}
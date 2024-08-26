using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class EditNotePage : IRefreshableView<EditNoteViewModel>
{
    public EditNotePage(EditNoteViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }

    public EditNoteViewModel Model { get; set; }
}
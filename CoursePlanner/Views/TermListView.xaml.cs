using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Views;

public partial class TermListView : ContentView, IPageModel<TermViewModel>
{
    public TermListView(TermViewModel model)
    {
        Model = model;
        InitializeComponent();
    }

    public TermViewModel Model { get; set; }
}
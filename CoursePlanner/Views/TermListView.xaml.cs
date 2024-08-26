using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Views;

public partial class TermListView : IRefreshableView<TermViewModel>
{
    public TermListView(TermViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;
    }

    public TermViewModel Model { get; set; }
}
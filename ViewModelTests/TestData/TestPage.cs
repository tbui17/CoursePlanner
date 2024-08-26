using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace ViewModelTests.TestData;

public class TestPage : ContentPage, IRefreshableView<TermViewModel>
{
    public TermViewModel Model { get; } = null!;
}

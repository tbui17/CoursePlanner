using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace ViewModelTests.TestData;

public class TestPage : ContentPage, IRefreshableView<TermViewModel>
{
    public TermViewModel Model { get; } = CreateMock<TermViewModel>();
}


public class TestPage2 : ContentPage, IRefreshableView<EditTermViewModel>
{
    public EditTermViewModel Model { get; } = CreateMock<EditTermViewModel>();
}
using ViewModels.Domain;
using ViewModels.Interfaces;
using ViewModels.Domain;

namespace ViewModelTests.TestData;

public class TestPage : ContentPage, IRefreshableView<TermViewModel>
{
    public TermViewModel Model { get; } = CreateMock<TermViewModel>().Object;
}


public class TestPage2 : ContentPage, IRefreshableView<EditTermViewModel>
{
    public EditTermViewModel Model { get; } = CreateMock<EditTermViewModel>().Object;
}
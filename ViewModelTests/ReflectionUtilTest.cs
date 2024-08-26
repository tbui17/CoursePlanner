using FluentAssertions;
using Moq;
using ViewModels.Interfaces;
using ViewModels.PageViewModels;
using ViewModels.Utils;

namespace ViewModelTests;

file class TestPage : ContentPage, IRefreshableView<TermViewModel>
{
    public TermViewModel Model { get; } = new Mock<TermViewModel>().Object;
}

public class ReflectionUtilTest
{
    [Test]
    public void GetRefreshableViews_TermViewModel_ReturnsTestPage()
    {
        var util = new ReflectionUtil
        {
            AssemblyNames = ["ViewModels", "ViewModelTests"]
        };
        var res = util.GetRefreshableViews(typeof(TermViewModel));
        res.Should().Contain(typeof(TestPage)).And.ContainSingle();
    }
}
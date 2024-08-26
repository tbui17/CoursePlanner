using FluentAssertions;
using ViewModels.PageViewModels;
using ViewModels.Utils.ReflectUtils;
using ViewModelTests.TestData;

namespace ViewModelTests;

public class ReflectionUtilTest
{
    [Test]
    public void GetRefreshableViews_TermViewModel_ReturnsTestPage()
    {
        var util = new ReflectionUtil
        {
            AssemblyNames = [nameof(ViewModels), nameof(ViewModelTests)]
        };
        var res = util.GetRefreshableViewsContainingTarget(typeof(TermViewModel));
        res.Should().Contain(typeof(TestPage)).And.ContainSingle();
    }
}
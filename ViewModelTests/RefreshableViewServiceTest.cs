using FluentAssertions;
using Lib.Services;
using Microsoft.Extensions.Caching.Memory;
using ViewModels.Domain;
using ViewModels.Domain;
using ViewModelTests.TestData;
using ViewModelTests.TestSetup;


namespace ViewModelTests;

public class RefreshableViewServiceTest : BaseTest
{
    [Test]
    public void GetRefreshableViews_TermViewModel_ReturnsTestPage()
    {
        var util = Resolve<RefreshableViewService>();
        var res = util.GetRefreshableViewsContainingTarget(typeof(TermViewModel));
        res.Should().Contain(typeof(TestPage)).And.ContainSingle();
    }

    [Test]
    public void InitializeCache_CachesResults()
    {
        var cache = Resolve<IMemoryCache>();
        cache.Get<IEnumerable<Type>>(typeof(TermViewModel)).Should().BeNull();
        cache.Get<IEnumerable<Type>>(typeof(EditTermViewModel)).Should().BeNull();
        var util = Resolve<RefreshableViewService>();
        util.InitializeCache();
        cache.Get<IEnumerable<Type>>(typeof(TermViewModel)).Should().NotBeNull();
        cache.Get<IEnumerable<Type>>(typeof(EditTermViewModel)).Should().NotBeNull();
    }
}
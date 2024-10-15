using BuildLib.SolutionBuild;
using BuildLib.Utils;
using BuildTests.TestSetup;
using FluentAssertions;
using Xunit.Abstractions;

namespace BuildTests.BuildProject;

public class SolutionTest(ITestOutputHelper helper) : BaseContainerSetup(helper)
{
    [Fact]
    public void ReleaseProject_Resolves()
    {
        var act = Resolve<ReleaseProject>;
        act.Should().NotThrow();
    }


    [Fact]
    public void GetAppVersion_ShouldReturnValidSemVersion()
    {
        var act = Resolve<MsBuildProject>;
        var msProj = act.Should().NotThrow().Subject;
        var act2 = msProj.GetAppVersion;
        var version = act2.Should().NotThrow().Subject;
        new[] { version.Major, version.Minor, version.Patch }
            .Should()
            .AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(0))
            .And.Contain(x => x > 0);
    }
}
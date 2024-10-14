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
}
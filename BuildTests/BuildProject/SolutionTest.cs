using BuildLib.SolutionBuild;
using BuildLib.SolutionBuild.Versioning;
using BuildLib.Utils;
using BuildTests.TestSetup;
using FluentAssertions;
using Xunit.Abstractions;
using ProjectVersionDataMapper = BuildLib.SolutionBuild.Versioning.ProjectVersionDataMapper;

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
        var act2 = msProj.GetAppDisplayVersion;
        var version = act2.Should().NotThrow().Subject;
        new[] { version.Major, version.Minor, version.Patch }
            .Should()
            .AllSatisfy(x => x.Should().BeGreaterThanOrEqualTo(0))
            .And.Contain(x => x > 0);
    }

    [Fact]
    public void Run()
    {
        var data = new ProjectVersionData() { AppVersion = new(1, 0, 0), VersionCode = 5 };
        var mapped = new ProjectVersionDataMapper().ToValidatedProjectVersionData(data);
        mapped.Should().BeOfType<ValidatedProjectVersionData>();
        mapped.AppVersion.Should().Be(data.AppVersion);
        mapped.VersionCode.Should().Be(data.VersionCode);
    }
}
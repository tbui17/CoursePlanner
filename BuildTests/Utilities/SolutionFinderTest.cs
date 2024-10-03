using BuildLib.FileSystem;
using FluentAssertions;

namespace BuildTests.Utilities;

public class SolutionFinderTest
{
    [Fact]
    public void FindSolutionFile_GetsSolution()
    {
        const string solutionFileName = "CoursePlanner.sln";
        var res = new SolutionFinder(solutionFileName).FindSolutionFile();
        res.Should().NotBeNull();
        res.Name.Should().Be(solutionFileName);
    }
}
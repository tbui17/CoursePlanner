using BuildLib.FileSystem;
using BuildTests.Utils;
using FluentAssertions;

namespace BuildTests.Utilities;

public class SolutionFinderTest
{
    [Fact]
    public void FindSolutionFile_GetsSolution()
    {
        var container = new ContainerInitializer().GetContainer();
        var dir = container.Resolve<DirectoryService>();
        var fac = container.Resolve<FileArgFactory>();

        const string solutionFileName = "CoursePlanner.sln";
        var act = () => dir.GetOrThrowSolutionFile(fac.CreateSolution(new() { FileName = solutionFileName }));

        act.Should().NotThrow().Which.Name.Should().Be(solutionFileName);
    }
}
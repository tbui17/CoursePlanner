using BuildLib.FileSystem;
using BuildTests.Attributes;
using BuildTests.Utils;
using FluentAssertions;

namespace BuildTests.Utilities;

public class SolutionFinderTest
{
    [Integration]
    public void FindSolutionFile_GetsSolution()
    {
        var container = new ContainerInitializer().GetContainer();
        var dir = container.Resolve<DirectoryService>();

        const string solutionFileName = "CoursePlanner.sln";
        var act = () =>
            dir.GetOrThrowSolutionFile(dir.FileArgFactory.CreateSolution(new() { FileName = solutionFileName }));

        act.Should().NotThrow().Which.Name.Should().Be(solutionFileName);
    }
}
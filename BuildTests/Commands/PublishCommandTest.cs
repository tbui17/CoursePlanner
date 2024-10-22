using BuildLib.Commands;
using BuildLib.Utils;
using BuildTests.Attributes;
using BuildTests.TestSetup;
using FluentAssertions;
using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Xunit.Abstractions;

namespace BuildTests.Commands;

[TestSubject(typeof(PublishCommand))]
public class PublishCommandTest : BaseContainerSetup
{
    public PublishCommandTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        var solution = Container.Resolve<Solution>();
        solution.CleanOutputDirectory();
    }


    [ManualTest(Timeout = 1000 * 3600)]
    public async Task DotNetPublish_ShouldCreateReleaseFile()
    {
        var solution = Container.Resolve<Solution>();
        var command = Container.Resolve<PublishCommand>();
        await command.ExecuteAsync();

        solution
            .Directory
            .GetDirectories()
            .Should()
            .ContainSingle(x => x.Name.EqualsIgnoreCase("output"))
            .Which.GetFiles()
            .Should()
            .ContainSingle(x => x.Name.ContainsIgnoreCase("signed"));
    }
}
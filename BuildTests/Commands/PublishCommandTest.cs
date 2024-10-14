using BuildLib.Commands;
using BuildLib.Utils;
using BuildTests.Attributes;
using BuildTests.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;
using Xunit.Abstractions;

namespace BuildTests.Commands;

[TestSubject(typeof(PublishCommand))]
public class PublishCommandTest
{
    private readonly Container _container;

    public PublishCommandTest(ITestOutputHelper testOutputHelper)
    {
        _container = new ContainerInitializer().GetContainer();
        var solution = _container.Resolve<Solution>();
        solution.GetOutputDirectory().DeleteDirectory();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateLogger();
    }

    [SkipIfDev(Timeout = 1000 * 3600)]
    public async Task DotNetPublish_ShouldCreateReleaseFile()
    {
        var solution = _container.Resolve<Solution>();
        var command = _container.Resolve<PublishCommand>();
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
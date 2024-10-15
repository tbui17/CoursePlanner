using BuildLib.AndroidPublish;
using BuildLib.SolutionBuild;
using BuildTests.TestSetup;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace BuildTests.Commands;

[TestSubject(typeof(PublishService))]
public class PublishServiceTest : BaseContainerSetup
{
    public PublishServiceTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void CreateDotNetPublishSettings_ShouldInitializeWithRequiredProperties()
    {
        var service = Resolve<PublishService>();
        var act = service.CreateDotNetPublishSettings;
        var settings = act.Should().NotThrow().Subject;
        settings.Framework.Should().Contain("android");
        settings.Properties.Should().ContainKey(nameof(AndroidSigningKeyStoreOptions.AndroidSigningKeyPass));
        settings.Project.Should().Contain("CoursePlanner.csproj").And.NotContain("/bin/");
    }
}
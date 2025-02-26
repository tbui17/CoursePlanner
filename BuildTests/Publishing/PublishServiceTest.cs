using BuildLib.AndroidPublish;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using BuildTests.TestSetup;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace BuildTests.Publishing;

[TestSubject(typeof(PublishService))]
public class PublishServiceTest : BaseContainerSetup
{
    public PublishServiceTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void CreateDotNetPublishSettings_ShouldInitializeWithRequiredProperties()
    {
        var config = Container.Services.GetAppConfigurationOrThrow(x => x.DotnetPublishAndroidConfiguration);
        var service = Resolve<DotNetPublishSettingsFactory>();
        var act = service.Create;
        var settings = act.Should().NotThrow().Subject;
        settings.Framework.Should().Contain("android");
        settings.Properties.Should().ContainKey(nameof(AndroidSigningKeyStoreOptions.AndroidSigningKeyPass));
        settings.Project.Should().Contain($"{config.ProjectName}").And.NotContain("/bin/");
    }
}
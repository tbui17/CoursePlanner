using BuildLib.AndroidPublish;
using BuildLib.Commands;
using BuildTests.Attributes;
using BuildTests.TestSetup;
using BuildTests.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace BuildTests.Commands;

[TestSubject(typeof(PublishService))]
public class PublishServiceTest(ITestOutputHelper testOutputHelper) : BaseContainerSetup(testOutputHelper)
{
    [SkipIfDev]
    public void CreateDotNetPublishSettings_ShouldInitializeWithRequiredProperties()
    {
        var service = new ContainerInitializer().GetContainer().Resolve<PublishService>();
        var settings = service.CreateDotNetPublishSettings();
        settings.Framework.Should().Contain("android");
        settings.Properties.Should().ContainKey(nameof(AndroidSigningKeyStoreOptions.AndroidSigningKeyPass));
        settings.Project.Should().Contain("CoursePlanner.csproj").And.NotContain("/bin/");
    }
}
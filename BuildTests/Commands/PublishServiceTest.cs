using BuildLib.AndroidPublish;
using BuildLib.Commands;
using BuildTests.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Serilog;
using Xunit.Abstractions;

namespace BuildTests.Commands;

[TestSubject(typeof(PublishService))]
public class PublishServiceTest
{
    public PublishServiceTest(ITestOutputHelper testOutputHelper)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.TestOutput(testOutputHelper)
            .CreateLogger();
    }

    [Fact]
    public void CreateDotNetPublishSettings_ShouldInitializeWithRequiredProperties()
    {
        var service = new ContainerInitializer().GetContainer().Resolve<PublishService>();
        var settings = service.CreateDotNetPublishSettings("CoursePlanner.sln", "CoursePlanner.csproj");
        settings.Framework.Should().Contain("android");
        settings.Properties.Should().ContainKey(nameof(AndroidSigningKeyStoreOptions.AndroidSigningKeyPass));
        settings.Project.Should().Contain("CoursePlanner.csproj").And.NotContain("/bin/");
    }
}
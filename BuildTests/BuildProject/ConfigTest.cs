using BuildLib.Utils;
using BuildTests.Utils;
using FluentAssertions;

namespace BuildTests.BuildProject;

public class ConfigTest
{
    private readonly Container _container = new ContainerInitializer().GetContainer();

    [Fact]
    public void CanResolveConfig()
    {
        var config = _container.Services.GetAppConfigurationOrThrow(x => x.AzureKeyVaultConfiguration);
        config.Uri.Should().NotBeNullOrWhiteSpace();
    }
}
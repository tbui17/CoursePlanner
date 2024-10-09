using Azure.Security.KeyVault.Secrets;
using BuildLib.Clients;
using BuildLib.Secrets;
using BuildLib.Utils;
using BuildTests.Utils;
using FluentAssertions;

namespace BuildTests.BuildProject;

public class ConnectionTest
{
    private readonly Container _container = new ContainerInitializer().GetContainer();


    [Fact(Skip = "Broken")]
    public void SecretClient_CanConnect()
    {
        const string name = nameof(CoursePlannerConfiguration.KeyUri);

        var client = _container.Resolve<SecretClient>();
        client
            .GetSecret(name)
            .Value.Name.Should()
            .Be(name);
    }

    [Fact]
    public async Task AndroidPublisherClient_CanConnect()
    {
        var client = _container.Resolve<AndroidPublisherClient>();


        var res = await client
            .Awaiting(x => x.Probe())
            .Should()
            .NotThrowAsync();

        res.Which.Id.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetBundles_HasResult()
    {
        var secrets = _container.GetConfig<CoursePlannerConfiguration>();
        secrets.Validate();
        var client = _container.Resolve<AndroidPublisherClient>();


        var res = await client
            .Awaiting(x => x.GetBundles())
            .Should()
            .NotThrowAsync();

        res
            .Which.Bundles.Should()
            .NotBeNullOrEmpty()
            .And.Contain(x => x.VersionCode == 1);
    }
}
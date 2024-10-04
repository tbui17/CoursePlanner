using Azure.Security.KeyVault.Secrets;
using BuildLib.Clients;
using BuildLib.Secrets;
using BuildLib.Utils;
using BuildTests.Attributes;
using BuildTests.Utils;
using FluentAssertions;

namespace BuildTests.BuildProject;

public class ConnectionTest
{
    private readonly Container _container = new ContainerInitializer().GetContainer();


    [SkipIfDev]
    public void SecretClient_CanConnect()
    {
        const string name = nameof(CoursePlannerSecrets.KeyUri);

        var client = _container.Resolve<SecretClient>();
        client
            .GetSecret(name)
            .Value.Name.Should()
            .Be(name);
    }

    [SkipIfDev]
    public async Task AndroidPublisherClient_CanConnect()
    {
        var client = _container.Resolve<AndroidPublisherClient>();


        var res = await client
            .Awaiting(x => x.Probe())
            .Should()
            .NotThrowAsync();

        res.Which.Id.Should().NotBeNullOrWhiteSpace();
    }

    [SkipIfDev]
    public async Task GetBundles_HasResult()
    {
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
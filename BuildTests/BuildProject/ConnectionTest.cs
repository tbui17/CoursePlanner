using BuildLib.CloudServices.GooglePlay;
using BuildLib.Secrets;
using BuildLib.Utils;
using BuildTests.Attributes;
using BuildTests.Utils;
using FluentAssertions;

namespace BuildTests.BuildProject;

public class ConnectionTest
{
    private readonly Container _container = new ContainerInitializer().GetContainer();


    [Integration]
    public void SecretClient_CanConnect()
    {
        var config = _container.GetConfiguration<AppConfiguration>();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        var keyIsNull = config.Key is null;

        keyIsNull.Should().NotBe(true);
    }

    [Integration]
    public async Task AndroidPublisherClient_CanConnect()
    {
        var client = _container.Resolve<IAndroidPublisherClient>();


        var res = await client
            .Awaiting(x => x.Probe())
            .Should()
            .NotThrowAsync();

        res.Which.Id.Should().NotBeNullOrWhiteSpace();
    }

    [Integration]
    public async Task GetBundles_HasResult()
    {
        var secrets = _container.GetConfiguration<AppConfiguration>();
        secrets.ValidateOrThrow();
        var client = _container.Resolve<IAndroidPublisherClient>();


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
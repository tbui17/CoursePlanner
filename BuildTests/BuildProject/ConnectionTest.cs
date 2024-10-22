using BuildLib.CloudServices.GooglePlay;
using BuildLib.Secrets;
using BuildTests.Attributes;
using BuildTests.TestSetup;
using FluentAssertions;
using Xunit.Abstractions;

namespace BuildTests.BuildProject;

public class ConnectionTest(ITestOutputHelper testOutputHelper) : BaseContainerSetup(testOutputHelper)
{
    [Integration]
    public void SecretClient_CanConnect()
    {
        var config = GetConfiguration<AppConfiguration>();

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        var keyIsNull = config.Key is null;

        keyIsNull.Should().NotBe(true);
    }

    [Integration]
    public async Task AndroidPublisherClient_CanConnect()
    {
        var client = Resolve<IAndroidPublisherClient>();


        var res = await client
            .Awaiting(x => x.Probe())
            .Should()
            .NotThrowAsync();

        res.Which.Should().NotBeNullOrWhiteSpace();
    }

    [Integration]
    public async Task GetBundles_HasResult()
    {
        var secrets = GetConfiguration<AppConfiguration>();
        secrets.ValidateOrThrow();
        var client = Resolve<IAndroidPublisherClient>();


        var res = await client
            .Awaiting(x => x.GetBundles())
            .Should()
            .NotThrowAsync();

        res
            .Which.Bundles.Should()
            .NotBeNullOrEmpty()
            .And.Contain(x => x.VersionCode > 0);
    }
}
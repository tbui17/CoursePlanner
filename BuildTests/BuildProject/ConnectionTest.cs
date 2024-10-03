using Azure.Security.KeyVault.Secrets;
using BuildLib.Clients;
using BuildLib.Secrets;
using BuildLib.Utils;
using BuildTests.Attributes;
using FluentAssertions;
using Microsoft.Extensions.Hosting;

namespace BuildTests.BuildProject;

public class ConnectionTest
{
    Container GetContainer() => Container.Init<Build>();
    HostApplicationBuilder GetBuilder() => Container.CreateBuilder<Build>();

    [SkipIfDev]
    public void SecretClient_CanConnect()
    {
        const string name = nameof(CoursePlannerSecrets.KeyUri);
        var container = GetContainer();
        var client = container.Resolve<SecretClient>();
        client
            .GetSecret(name)
            .Value.Name.Should()
            .Be(name);
    }

    [SkipIfDev]
    public void AndroidPublisherClient_CanConnect()
    {
        var container = GetContainer();
        var client = container.Resolve<AndroidPublisherClient>();

        var req = client.ProbeInsertRequest();


        req
            .Invoking(x => x.Execute())
            .Should()
            .NotThrow()
            .Which.Id.Should()
            .NotBeNullOrWhiteSpace();
    }
}
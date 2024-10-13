using BuildLib.CloudServices.AzureBlob;
using BuildTests.Attributes;
using BuildTests.Utils;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit.Abstractions;
using Version = BuildLib.CloudServices.AzureBlob.Version;

namespace BuildTests.Clients;

[TestSubject(typeof(AzureBlobClient))]
public class AzureBlobClientTest
{
    private const string TestId = "d16234b3-b35f-4069-a9ea-fa086f822086";
    private readonly ITestOutputHelper _testOutputHelper;

    public AzureBlobClientTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [SkipIfDev]
    public async Task GetBlobNames_HasResults()
    {
        var client = new ContainerInitializer()
            .GetContainer()
            .Resolve<AzureBlobClient>();

        var names = await client.GetBlobNames();
        names.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DownloadBlob_Succeeds()
    {
        var client = new ContainerInitializer()
            .GetContainer()
            .Resolve<AzureBlobClient>();

        var names = await client.GetBlobNames();
        var blobName = names.First();
        const string path = TestId + "test_file.aab";
        var opts = new DownloadBlobOptions
        {
            BlobName = blobName,
            Path = path,
            ProgressHandler = x => _testOutputHelper.WriteLine($"Downloaded {x} bytes")
        };
        await client.Awaiting(x => x.DownloadBlob(opts)).Should().NotThrowAsync();
        var file = new FileInfo(path);
        file.Exists.Should().Be(true);
    }

    [Fact]
    public async Task GetLatestApbFile_ReturnsLatestApbFile()
    {
        var client = new ContainerInitializer()
            .GetContainer()
            .Resolve<AzureBlobClient>();

        var res = await client.GetLatestAabFileGlob();

        res.Should().NotBeNull();

        res.Version.Should().Be(new Version { Major = 0, Minor = 0, Patch = 2 });
    }

    [Fact]
    public async Task DownloadLatestApbFile_CreatesApbFile()
    {
        var client = new ContainerInitializer()
            .GetContainer()
            .Resolve<AzureBlobClient>();

        var res = await client.DownloadLatestAabFile();
        res.BlobName.Should().EndWith(".aab");
    }
}
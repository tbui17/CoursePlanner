using BuildLib.CloudServices.AzureBlob;
using BuildLib.Utils;
using BuildTests.Attributes;
using BuildTests.TestSetup;
using FluentAssertions;
using JetBrains.Annotations;
using Nuke.Common.ProjectModel;
using Semver;
using Xunit.Abstractions;

namespace BuildTests.Clients;

[TestSubject(typeof(IBlobClient))]
public class BlobClientTest : BaseContainerSetup
{
    private const string TestId = "d16234b3-b35f-4069-a9ea-fa086f822086";

    public BlobClientTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [SkipIfDev]
    public async Task GetBlobNames_HasResults()
    {
        var client = Resolve<IBlobClient>();

        var names = await client.GetBlobNames();
        names.Should().NotBeNullOrEmpty();
    }

    [SkipIfDev]
    public async Task DownloadBlob_Succeeds()
    {
        var client = Resolve<IBlobClient>();

        var names = await client.GetBlobNames();
        var blobName = names.First();
        const string path = TestId + "test_file.aab";
        var opts = new DownloadBlobOptions
        {
            BlobName = blobName,
            Path = path
        };
        await client.Awaiting(x => x.DownloadBlob(opts)).Should().NotThrowAsync();
        var file = new FileInfo(path);
        file.Exists.Should().Be(true);
    }

    [Fact]
    public async Task GetLatestApbFile_ReturnsLatestApbFile()
    {
        var client = Resolve<AabCacheClient>();

        var res = await client.GetLatestAabFileGlob();

        res.Should().NotBeNull();

        res.Version.Should().Be(new SemVersion(0, 0, 2));
    }

    [SkipIfDev]
    public async Task DownloadLatestApbFile_CreatesApbFile()
    {
        var client = Resolve<AabCacheClient>();

        var res = await client.DownloadLatestAabFile();
        res.BlobName.Should().EndWith(".aab");
    }

    [SkipIfDev]
    public async Task UploadAabFile_Succeeds()
    {
        var client = Resolve<AabCacheClient>();
        var solution = Resolve<Solution>();

        var path = solution.GetSignedAabFile();

        await using var fileStream = File.Open(path, FileMode.Open);

        await client.UploadAabFile(fileStream, new CancellationTokenSource(60000).Token);
    }
}
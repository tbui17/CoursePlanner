using BuildLib.CloudServices.AzureBlob;
using BuildLib.SolutionBuild.Versioning;
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

    [Integration]
    public async Task GetBlobNames_HasResults()
    {
        var client = Resolve<IBlobClient>();

        var names = await client.GetBlobNames();
        names.Should().NotBeNullOrEmpty();
    }

    [Integration]
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

    [Integration]
    public async Task GetLatestApbFile_ReturnsNonEmptySemanticVersion()
    {
        var client = Resolve<AabCacheClient>();

        var blob = await client.GetLatestAabFileBlob();

        blob.Should().NotBeNull();

        blob.Version.Should().Match<SemVersion>(x => x.ContainsNonZeroPositiveValue());
    }

    [Integration]
    public async Task DownloadLatestApbFile_CreatesApbFile()
    {
        var client = Resolve<AabCacheClient>();

        var res = await client.DownloadLatestAabFile();
        res.BlobName.Should().EndWith(".aab");
    }

    [Integration]
    public async Task UploadAabFile_Succeeds()
    {
        var client = Resolve<AabCacheClient>();
        var solution = Resolve<Solution>();

        var path = solution.GetSignedAabFile();

        await using var fileStream = File.Open(path, FileMode.Open);

        await client.UploadAabFile(fileStream, new CancellationTokenSource(60000).Token);
    }
}
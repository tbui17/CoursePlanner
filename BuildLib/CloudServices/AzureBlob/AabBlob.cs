using Azure.Storage.Blobs.Models;
using Semver;

namespace BuildLib.CloudServices.AzureBlob;

public interface IBlob
{
    string Name { get; }
    string Path { get; }
    SemVersion Version { get; }
    IBlob WithVersion(Func<SemVersion, SemVersion> versionFunc);
}

public record AzureBlob : IBlob
{
    public required BlobItem Blob { get; init; }
    public required SemVersion Version { get; init; }

    public IBlob WithVersion(Func<SemVersion, SemVersion> versionFunc)
    {
        return this with { Version = versionFunc(Version) };
    }

    public string Path => Blob.Name;
    public string Name => Blob.Name.Split("/").Last();
}

public static class DownloadBlobExtensions
{
    public static IDownloadBlobOptions ToDownloadBlobOptions(this AzureBlob fileBlob)
    {
        return new DownloadBlobOptions
        {
            BlobName = fileBlob.Blob.Name,
            Path = fileBlob.Name,
        };
    }
}
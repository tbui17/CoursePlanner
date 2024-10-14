using Azure.Storage.Blobs.Models;

namespace BuildLib.CloudServices.AzureBlob;

public interface IBlob
{
    string Name { get; }
    string Path { get; }
    Version Version { get; }
    IBlob WithVersion(Func<Version, Version> versionFunc);
}

public record AzureBlob : IBlob
{
    public required BlobItem Blob { get; init; }
    public required Version Version { get; init; }

    public IBlob WithVersion(Func<Version, Version> versionFunc)
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
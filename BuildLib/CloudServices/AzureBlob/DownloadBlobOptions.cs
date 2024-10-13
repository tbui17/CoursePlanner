namespace BuildLib.CloudServices.AzureBlob;

public interface IDownloadBlobOptions
{
    string BlobName { get; }
    string Path { get; }
    Action<long>? ProgressHandler { get; }
    int Timeout { get; }
}

public record DownloadBlobOptions : IDownloadBlobOptions
{
    public required string BlobName { get; init; }
    public required string Path { get; init; }
    public Action<long>? ProgressHandler { get; init; }
    public int Timeout { get; init; } = 60000;
}

public static class DownloadBlobExtensions
{
    public static IDownloadBlobOptions ToDownloadBlobOptions(this Aabblob fileBlob)
    {
        return new DownloadBlobOptions
        {
            BlobName = fileBlob.Blob.Name,
            Path = fileBlob.Name,
        };
    }
}
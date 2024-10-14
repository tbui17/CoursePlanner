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
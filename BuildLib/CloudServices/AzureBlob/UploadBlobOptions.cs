namespace BuildLib.CloudServices.AzureBlob;

public interface IUploadBlobOptions
{
    IBlob Blob { get; }
    Stream Stream { get; }
    CancellationToken Token { get; }
}

public record UploadBlobOptions : IUploadBlobOptions
{
    public required IBlob Blob { get; init; }
    public required Stream Stream { get; init; }
    public required CancellationToken Token { get; init; }
}
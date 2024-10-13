using Azure.Storage.Blobs.Models;

namespace BuildLib.CloudServices.AzureBlob;

public class Aabblob
{
    public required BlobItem Blob { get; init; }
    public required Version Version { get; init; }
}
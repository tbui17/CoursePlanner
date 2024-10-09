using Azure.Storage.Blobs;

namespace BuildLib.Clients;

public class AzureBlobClient(BlobContainerClient client)
{
    public async Task<List<string>> GetBlobNames()
    {
        var res = await client
            .GetBlobsAsync()
            .Select(x => x.Name)
            .Take(10)
            .ToListAsync();

        return res;
    }
}
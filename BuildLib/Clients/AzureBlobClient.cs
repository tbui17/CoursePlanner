using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace BuildLib.Clients;

public class AzureBlobClient(BlobContainerClient client, ILogger<AzureBlobClient> logger)
{
    public async Task<List<string>> GetBlobNames()
    {
        logger.LogInformation("Fetching blob names");
        var res = await client
            .GetBlobsAsync()
            .Select(x => x.Name)
            .Take(10)
            .ToListAsync();

        logger.LogInformation("Fetched blob names: {@BlobNames}", res);

        return res;
    }

    public async Task DownloadBlob(DownloadBlobOptions options)
    {
        var handler = options.ProgressHandler + (x => logger.LogInformation("Downloaded {Bytes} bytes", x));
        var opts = new BlobDownloadToOptions
        {
            ProgressHandler = new Progress<long>(handler)
        };

        var cts = new CancellationTokenSource(options.Timeout);
        logger.LogInformation("Beginning download with arguments: {@Options}", options);
        try
        {
            await client.GetBlobClient(options.BlobName).DownloadToAsync(options.Path, opts, cts.Token);
            logger.LogInformation("Download complete");
        }
        catch (RequestFailedException e) when (e.InnerException is TaskCanceledException)
        {
            throw new TimeoutException($"Download timed out after {options.Timeout} milliseconds", e);
        }
    }
}

public record DownloadBlobOptions
{
    public required string BlobName { get; init; }
    public required string Path { get; init; }
    public Action<long> ProgressHandler { get; init; } = _ => { };
    public int Timeout { get; init; } = 60000;
}
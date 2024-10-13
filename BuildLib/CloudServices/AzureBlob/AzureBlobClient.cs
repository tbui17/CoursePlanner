using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BuildLib.Exceptions;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;

namespace BuildLib.CloudServices.AzureBlob;

[Inject]
public class AzureBlobClient(BlobContainerClient client, ILogger<AzureBlobClient> logger)
{
    private const string AabExtension = ".aab";
    private const string VersionTag = "version";

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

    private AsyncPageable<BlobItem> GetBlobs()
    {
        logger.LogInformation("Fetching blob names");
        var res = client
            .GetBlobsAsync(BlobTraits.Tags);

        return res;
    }

    public IAsyncEnumerable<Aabblob> GetAabFiles()
    {
        return GetBlobs()
            .Where(x => x.Name.EndsWith(AabExtension))
            .Select(x =>
                {
                    if (!x.Tags.TryGetValue(VersionTag, out var str))
                    {
                        return null;
                    }

                    var version = Version.Parse(str);

                    return new Aabblob
                    {
                        Blob = x,
                        Version = version
                    };
                }
            )
            .OfType<Aabblob>()
            .Take(10000);
    }

    public async Task<Aabblob?> GetLatestAabFileGlob()
    {
        var res = await GetAabFiles()
            .MaxByAsync(x => x.Version);

        if (res.Count <= 1) return res.SingleOrDefault();
        throw DuplicateVersionException.Create(res);
    }

    public async Task<IDownloadBlobOptions> DownloadLatestAabFile()
    {
        var res = await GetLatestAabFileGlob();
        if (res is null)
        {
            throw new InvalidOperationException("No files found");
        }

        var opts = res.ToDownloadBlobOptions();

        await DownloadBlob(opts);
        return opts;
    }


    public async Task DownloadBlob(IDownloadBlobOptions options)
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
            logger.LogInformation("Download complete. Saved to {Path}", options.Path);
        }
        catch (RequestFailedException e) when (e.InnerException is TaskCanceledException)
        {
            throw new TimeoutException($"Download timed out after {options.Timeout} milliseconds", e);
        }
    }
}
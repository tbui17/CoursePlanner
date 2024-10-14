using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BuildLib.Globals;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;

namespace BuildLib.CloudServices.AzureBlob;

public interface IBlobClient
{
    Task<List<string>> GetBlobNames();
    Task DownloadBlob(IDownloadBlobOptions options);
    public IAsyncEnumerable<BlobItem> GetBlobs(CancellationToken token = default);
    Task UploadBlob(IUploadBlobOptions options);
}

[Inject(typeof(IBlobClient))]
public class AzureBlobClient : IBlobClient
{
    private readonly BlobContainerClient _client;
    private readonly ILogger<AzureBlobClient> _logger;
    private readonly Progress<long> _progressHandler;
    private readonly Action<long> _progressHandlerImpl;

    public AzureBlobClient(BlobContainerClient client, ILogger<AzureBlobClient> logger)
    {
        _client = client;
        _logger = logger;
        _progressHandlerImpl = x => logger.LogDebug("Downloaded {Bytes} bytes", x);
        _progressHandler = new Progress<long>(_progressHandlerImpl);
    }

    public async Task<List<string>> GetBlobNames()
    {
        _logger.LogDebug("Fetching blob names");
        var res = await _client
            .GetBlobsAsync()
            .Select(x => x.Name)
            .Take(10)
            .ToListAsync();

        _logger.LogDebug("Fetched blob names: {@BlobNames}", res);

        return res;
    }

    public IAsyncEnumerable<BlobItem> GetBlobs(CancellationToken token = default)
    {
        _logger.LogDebug("Fetching blobs");
        return _client.GetBlobsAsync(BlobTraits.Tags | BlobTraits.Metadata, cancellationToken: token);
    }

    public async Task DownloadBlob(IDownloadBlobOptions options)
    {
        var handler = options.ProgressHandler + _progressHandlerImpl;
        var opts = new BlobDownloadToOptions
        {
            ProgressHandler = new Progress<long>(handler)
        };

        var cts = new CancellationTokenSource(options.Timeout);
        _logger.LogDebug("Beginning download with arguments: {@Options}", options);
        try
        {
            await _client.GetBlobClient(options.BlobName).DownloadToAsync(options.Path, opts, cts.Token);
            _logger.LogDebug("Download complete. Saved to {Path}", options.Path);
        }
        catch (RequestFailedException e) when (e.InnerException is TaskCanceledException)
        {
            throw new TimeoutException($"Download timed out after {options.Timeout} milliseconds", e);
        }
    }

    public async Task UploadBlob(IUploadBlobOptions options)
    {
        using var _ = _logger.MethodScope();
        _logger.LogDebug("Uploading blob with arguments: {@Options}", options);
        var opts = new BlobUploadOptions // do not set tags on opts directly, NRE
        {
            ProgressHandler = _progressHandler,
        };
        var tags = new Dictionary<string, string>
        {
            { Constants.VersionTag, options.Blob.Version }
        };
        var client = _client.GetBlockBlobClient(options.Blob.Path);

        var res = await client.UploadAsync(options.Stream, opts, cancellationToken: options.Token);

        _logger.LogDebug("Upload complete for {BlobPath} {@Data}", client.Name, res.Value);
        var res2 = await client.SetTagsAsync(tags);
        _logger.LogDebug("Tags set for {BlobPath} {@Tags} {Response}", client.Name, tags, res2);
    }
}
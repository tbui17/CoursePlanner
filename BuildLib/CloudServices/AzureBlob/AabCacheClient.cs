using BuildLib.Exceptions;
using BuildLib.Globals;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;

namespace BuildLib.CloudServices.AzureBlob;

public interface IAabCacheClient
{
    Task<IBlob?> GetLatestAabFileGlob();
    Task<IDownloadBlobOptions> DownloadLatestAabFile();
}

[Inject]
public class AabCacheClient(IBlobClient blobClient, ILogger<AabCacheClient> logger) : IAabCacheClient
{
    public async Task<IBlob?> GetLatestAabFileGlob()
    {
        var res = await GetAabFiles()
            .MaxByAsync(x => x.Version);

        logger.LogDebug("Found latest AAB file: {@Blob}", res);


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

        var opts = ((AzureBlob)res).ToDownloadBlobOptions();

        await blobClient.DownloadBlob(opts);
        return opts;
    }

    private IAsyncEnumerable<AzureBlob> GetAabFiles()
    {
        return blobClient
            .GetBlobs()
            .Where(x => x.Name.EndsWith(Constants.AabExtension))
            .Select(x =>
                {
                    if (x.VersionId is not null && Version.TryParse(x.VersionId, out var version))
                    {
                        return new AzureBlob
                        {
                            Blob = x,
                            Version = version,
                        };
                    }

                    if (x.Tags.TryGetValue(Constants.VersionTag, out var versionStr))
                    {
                        return new AzureBlob
                        {
                            Blob = x,
                            Version = Version.Parse(versionStr)
                        };
                    }

                    return null;
                }
            )
            .WhereNotNull()
            .Take(10000);
    }

    public async Task UploadAabFile(Stream stream, CancellationToken cancellationToken)
    {
        var blob = await GetLatestAabFileGlob() ?? throw new InvalidOperationException("No files found");

        var opts = new UploadBlobOptions
        {
            Blob = blob.WithVersion(x => x.UpdatePatch()),
            Stream = stream,
            Token = cancellationToken
        };

        await blobClient.UploadBlob(opts);
    }
}
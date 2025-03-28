using BuildLib.Exceptions;
using BuildLib.Globals;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Semver;

namespace BuildLib.CloudServices.AzureBlob;

public interface IAabCacheClient
{
    Task<IBlob?> GetLatestAabFileBlob();
    Task<IDownloadBlobOptions> DownloadLatestAabFile();
}

[Inject]
public class AabCacheClient(IBlobClient blobClient, ILogger<AabCacheClient> logger) : IAabCacheClient
{
    public async Task<IBlob?> GetLatestAabFileBlob()
    {
        var res = await GetAabFiles()
            .MaxByAsync(x => x.Version);

        logger.LogDebug("Found latest AAB file: {@Blob}", res);


        if (res.Count <= 1) return res.SingleOrDefault();
        throw DuplicateVersionException.Create(res);
    }


    public async Task<IDownloadBlobOptions> DownloadLatestAabFile()
    {
        var res = await GetLatestAabFileBlob();
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
                    if (x.VersionId is not null &&
                        SemVersion.TryParse(x.VersionId, SemVersionStyles.AllowV, out var version))
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
                            Version = SemVersion.Parse(versionStr, SemVersionStyles.AllowV),
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
        var blob = await GetLatestAabFileBlob() ?? throw new InvalidOperationException("No files found");

        var opts = new UploadBlobOptions
        {
            Blob = blob.WithVersion(x => x.UpdatePatch()),
            Stream = stream,
            Token = cancellationToken
        };

        await blobClient.UploadBlob(opts);
    }
}
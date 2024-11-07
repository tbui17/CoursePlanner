using BuildLib.Utils;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace BuildLib.Secrets;

public record DotnetPublishAndroidConfiguration
{
    // public string ApplicationId { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public string AndroidSigningKeyStore { get; set; } = null!;
    public string AndroidSigningKeyAlias { get; set; } = null!;
    public string AndroidSigningKeyPass { get; set; } = null!;
    public string KeystoreFile { get; set; } = null!;
    public string AndroidFramework { get; set; } = null!;
    public string Configuration { get; set; } = null!;
}

public record GooglePlayDeveloperApiConfiguration
{
    // public string ProjectName { get; set; } = null!;
    public string ReleaseTrack { get; set; } = null!;
    public string ReleaseStatus { get; set; } = null!;
    public GoogleClientKey GoogleClientKey { get; set; } = null!;
}

public interface IGooglePlayDeveloperApiConfigurationProxy
{
    public string PackageName { get; }
    public string ReleaseTrack { get; }
    public string ReleaseStatus { get; }
    public GoogleClientKey GoogleClientKey { get; }
}

public class GooglePlayDeveloperApiConfigurationProxy : IGooglePlayDeveloperApiConfigurationProxy
{
    public string PackageName { get; set; } = null!;
    public string ReleaseTrack { get; set; } = null!;
    public string ReleaseStatus { get; set; } = null!;
    public GoogleClientKey GoogleClientKey { get; set; } = null!;
}

public record AzureBlobStorageConfiguration
{
    public string ContainerName { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
}

public record AzureKeyVaultConfiguration
{
    public string Uri { get; set; } = null!;
}

public record AppConfiguration
{
    public DotnetPublishAndroidConfiguration DotnetPublishAndroidConfiguration { get; set; } = null!;
    public GooglePlayDeveloperApiConfiguration GooglePlayDeveloperApiConfiguration { get; set; } = null!;
    public AzureBlobStorageConfiguration AzureBlobStorageConfiguration { get; set; } = null!;
    public AzureKeyVaultConfiguration AzureKeyVaultConfiguration { get; set; } = null!;


    public NullDiffRecord GetNullDiff()
    {
        var props = this
            .GetPropertiesRecursive()
            .Partition(x => x.Value is string s
                ? string.IsNullOrWhiteSpace(s)
                : x.Value is null
            );

        return new NullDiffRecord { Nulls = props.True.ToList(), NotNulls = props.False.ToList() };
    }

    public ArgumentException? Validate()
    {
        var record = GetNullDiff();
        var nulls = record.Nulls;
        var notNulls = record.NotNulls;


        if (nulls.Count == 0) return null;

        var errorObj = new
        {
            Nulls = CreatePathList(nulls),
            NotNulls = CreatePathList(notNulls),
            Message = "Secrets are missing"
        };
        return new ArgumentException(JsonConvert.SerializeObject(errorObj, Formatting.Indented))
            { Data = { ["Data"] = errorObj } };

        IEnumerable<string> CreatePathList(IEnumerable<ObjectNode> nodes) =>
            nodes
                .Select(x => x.GetPath())
                .Select(x => string.Join(".", x));
    }

    public void ValidateOrThrow()
    {
        if (Validate() is { } ex)
        {
            throw ex;
        }
    }
}
using BuildLib.Utils;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace BuildLib.Secrets;

public record AppConfiguration
{
    public string AndroidSigningKeyStore { get; set; } = null!;
    public string AndroidSigningKeyAlias { get; set; } = null!;
    public string Key { get; set; } = null!;
    public GoogleServiceAccount GoogleServiceAccount { get; set; } = null!;
    public string BlobContainerName { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
    public string BlobConnectionString { get; set; } = null!;
    public string KeystoreContents { get; set; } = null!;
    public string ApplicationId { get; set; } = null!;
    public string GoogleServiceAccountBase64 { get; set; } = null!;
    public string KeyUri { get; set; } = null!;
    public string BundlePath { get; set; } = null!;
    public string ProjectName { get; set; } = null!;
    public string SolutionName { get; set; } = null!;
    public string AppVersion { get; set; } = null!;
    public string PublishConfiguration { get; set; } = null!;
    public string AndroidFramework { get; set; } = null!;
    public string AndroidSigningKeyPass { get; set; } = null!;


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
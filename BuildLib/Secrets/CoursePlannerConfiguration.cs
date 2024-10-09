using BuildLib.Utils;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace BuildLib.Secrets;

public record CoursePlannerConfiguration
{
    public string AndroidSigningKeyStore { get; set; } = null!;
    public string AndroidSigningKeyAlias { get; set; } = null!;
    public string Key { get; set; } = null!;
    public GoogleServiceAccount GoogleServiceAccount { get; set; } = null!;
    public string KeystoreContents { get; set; } = null!;
    public string ApplicationId { get; set; } = null!;
    public string GoogleServiceAccountBase64 { get; set; } = null!;
    public string KeyUri { get; set; } = null!;

    public void Validate()
    {
        var (nulls, notNulls) = this
            .GetPropertiesRecursive()
            .Partition(x => x.Value is string s
                ? string.IsNullOrWhiteSpace(s)
                : x.Value is null
            );

        var nullsList = nulls.ToList();

        if (nullsList.Count == 0) return;

        var errorObj = new
        {
            Nulls = CreatePathList(nullsList),
            NotNulls = CreatePathList(notNulls),
            Message = "Secrets are missing"
        };
        throw new ArgumentException(JsonConvert.SerializeObject(errorObj, Formatting.Indented))
            { Data = { ["Data"] = errorObj } };

        IEnumerable<string> CreatePathList(IEnumerable<ObjectNode> nodes) =>
            nodes
                .Select(x => x.GetPath())
                .Select(x => string.Join(".", x));
    }
}
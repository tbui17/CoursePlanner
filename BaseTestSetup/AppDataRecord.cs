using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Serilog;

namespace BaseTestSetup;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
internal record AppDataRecord
{
    public string ProjectName { get; set; } = "";
    public string Bin { get; set; } = "";
    public string Deployment { get; set; } = "";
    public string NetVersion { get; set; } = "";

    public static AppDataRecord Parse(string path)
    {
        var r = new AppDataRecord();
        List<Action<string>> setters =
        [
            s => r.NetVersion = s,
            s => r.Deployment = s,
            s => r.Bin = s,
            s => r.ProjectName = s
        ];

        var strings = path.Split("\\").Reverse().Where(x => !string.IsNullOrWhiteSpace(x));
        foreach (var (value, setter) in strings.Zip(setters))
        {
            setter(value);
        }

        return r;
    }

    private static ImmutableArray<PropertyPairFactory> PropertyGetters =>
    [
        ..typeof(AppDataRecord)
            .GetProperties()
            .Select(PropertyPairFactory (x) => record => (x.Name, x.GetValue(record)!))
    ];

    public void Enrich(LoggerConfiguration conf)
    {
        foreach (var (name, val) in PropertyGetters.Select(x => x(this)))
        {
            conf.Enrich.WithProperty(name, val);
        }
    }

    private delegate (string, object) PropertyPairFactory(AppDataRecord record);
}
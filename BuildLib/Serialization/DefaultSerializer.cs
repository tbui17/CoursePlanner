using BuildLib.Utils;
using Newtonsoft.Json;

namespace BuildLib.Serialization;

[Inject]
public class DefaultSerializer : ISerializer
{
    private readonly JsonSerializerSettings _settings = new()
    {
        MaxDepth = 100,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.Indented,
    };

    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }
}
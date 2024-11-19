using BuildLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildLib.Serialization;

[Inject]
public class SnakeCaseSerializer : ISerializer
{
    private readonly JsonSerializerSettings _settings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        }
    };

    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }
}
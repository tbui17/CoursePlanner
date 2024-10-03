using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BuildLib.Serialization;

public class SnakeCaseSerializer
{
    private readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new SnakeCaseNamingStrategy(),
        }
    };

    public string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, Settings);
    }
}
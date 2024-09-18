using Newtonsoft.Json;

namespace LibTests.ReportTests;

public static class UtilTestExtensions
{

    public static string Serialize(this object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public static void Dump(this object obj)
    {
        Console.WriteLine(obj.Serialize());
    }

    public static T Deserialize<T>(this string json)
    {
        return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings(){ReferenceLoopHandling = ReferenceLoopHandling.Ignore})!;
    }
}
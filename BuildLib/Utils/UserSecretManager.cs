using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace BuildLib.Utils;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class UserSecretManager<T>
{
    public ILogger Logger { get; init; } = Log.ForContext<UserSecretManager<T>>();

    private string GetUserSecretsId()
    {
        var assembly = Assembly.GetAssembly(typeof(T));
        if (assembly is null)
        {
            throw new InvalidOperationException("Assembly was null");
        }

        var attribute = assembly.GetCustomAttribute<UserSecretsIdAttribute>() ??
                        throw new InvalidOperationException("UserSecretsIdAttribute was null");
        Logger.Information("Retrieved: {UserSecretsId}", attribute.UserSecretsId);
        return attribute.UserSecretsId;
    }

    private FileInfo GetSecretJsonFile()
    {
        var jsonPath = GetJsonPath();
        var jsonPathResolved = Environment.ExpandEnvironmentVariables(jsonPath);
        var jsonPathResolvedInfo = new FileInfo(jsonPathResolved);
        return jsonPathResolvedInfo;
    }

    private string GetJsonPath()
    {
        var secretsId = GetUserSecretsId();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return $"~/.microsoft/usersecrets/{secretsId}/secrets.json";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return $@"%APPDATA%\Microsoft\UserSecrets\{secretsId}\secrets.json";
        }

        throw new PlatformNotSupportedException($"Platform not supported {RuntimeInformation.OSDescription}");
    }

    public UserSecretFile GetUserSecretFile()
    {
        var file = GetSecretJsonFile();
        var res = new UserSecretFile(
            file,
            File.ReadAllText(file.FullName).Thru(JObject.Parse)
        );
        return res;
    }

    public void Save(object obj)
    {
        var file = GetSecretJsonFile();
        Save(file, obj);
    }

    public void Save(UserSecretFile file)
    {
        File.WriteAllText(file.FileInfo.FullName, file.Data.ToString(Formatting.Indented));
        Logger.Information("Wrote secrets json to {Path}", file.FileInfo.FullName);
    }

    private void Save(FileInfo jsonFile, object obj)
    {
        File.WriteAllText(jsonFile.FullName, JsonConvert.SerializeObject(obj, Formatting.Indented));
        Logger.Information("Wrote secrets json to {Path}", jsonFile.FullName);
    }
}

public class UserSecretFile(FileInfo fileInfo, JObject data)
{
    public FileInfo FileInfo => fileInfo;

    public JObject Data { get; private set; } = data;

    public void Merge<T>(T obj) where T : class
    {
        var jObj = JObject.FromObject(obj);
        Data.Merge(jObj, new JsonMergeSettings { MergeNullValueHandling = MergeNullValueHandling.Ignore });
    }

    public void Overwrite(object obj)
    {
        Data = JObject.FromObject(obj);
    }
}
using BuildLib.Secrets;

namespace BuildLib.Globals;

public static class Constants
{
    public const string AabExtension = ".aab";
    public const string VersionTag = "version";

    public const string ConnectionStringKey =
        $"{nameof(AzureKeyVaultConfiguration)}--{nameof(AppConfiguration.AzureKeyVaultConfiguration.Uri)}";

    public const string AppConfigsKey = "AppConfigs";
}
using BuildLib.Secrets;

namespace BuildLib.Constant;

public static class Constants
{
    public const string ConnectionStringKey =
        $"{nameof(AzureKeyVaultConfiguration)}--{nameof(AppConfiguration.AzureKeyVaultConfiguration.ConnectionString)}";
}
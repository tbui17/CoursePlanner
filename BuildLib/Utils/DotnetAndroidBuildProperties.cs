namespace BuildLib.Utils;

public record DotnetAndroidBuildProperties
{
    public required string AndroidSigningKeyStore { get; init; }
    public required string AndroidSigningKeyAlias { get; init; }
    public required string AndroidSigningKeyPass { get; init; }
    public required string AndroidSigningStorePass { get; init; }
}
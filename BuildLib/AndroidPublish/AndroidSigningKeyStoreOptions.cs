using BuildLib.Utils;
using Namotion.Reflection;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.AndroidPublish;

public interface IAndroidSigningKeyStoreOptions
{
    string? AndroidSigningKeyStore { get; }
    string? AndroidSigningKeyAlias { get; }
    string? AndroidSigningKeyPass { get; }
    string? AndroidSigningStorePass { get; }
}

public record AndroidSigningKeyStoreOptions
{
    public required string AndroidSigningKeyStore { get; init; }
    public required string AndroidSigningKeyAlias { get; init; }
    public required string AndroidSigningKeyPass { get; init; }
    public required string AndroidSigningStorePass { get; init; }
    public bool AndroidKeyStore => true;
}

public static class AndroidSigningKeyStoreOptionsExtensions
{
    public static DotNetPublishSettings SetProperties(
        this DotNetPublishSettings settings,
        AndroidSigningKeyStoreOptions properties
    )
    {
        var dict = properties.ToPropertyDictionary();
        return settings.SetProperties(dict);
    }

    public static AndroidSigningKeyStoreOptions ToValidatedAndroidSigningKeyStoreOptions(
        this IAndroidSigningKeyStoreOptions dto
    )
    {
        var opts = new AndroidSigningKeyStoreOptions
        {
            AndroidSigningKeyStore = dto.AndroidSigningKeyStore!,
            AndroidSigningKeyAlias = dto.AndroidSigningKeyAlias!,
            AndroidSigningKeyPass = dto.AndroidSigningKeyPass!,
            AndroidSigningStorePass = dto.AndroidSigningStorePass!
        };
        opts.EnsureValidNullability();
        return opts;
    }
}
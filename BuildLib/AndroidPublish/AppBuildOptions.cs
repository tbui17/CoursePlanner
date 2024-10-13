namespace BuildLib.AndroidPublish;

/// <summary>
/// Represents the build options for an application.
/// </summary>
public class AppBuildOptions : IAndroidSigningKeyStoreOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppBuildOptions"/> class with default values.
    /// </summary>
    public AppBuildOptions()
    {
    }

    /// <summary>
    /// Gets or sets the user-visible name for the app.
    /// </summary>
    public string? ApplicationTitle { get; set; } // Corresponds to -p:ApplicationTitle

    /// <summary>
    /// Gets or sets the unique identifier for the app, such as com.companyname.mymauiapp.
    /// </summary>
    public string? ApplicationId { get; set; } // Corresponds to -p:ApplicationId

    /// <summary>
    /// Gets or sets the version of the build that identifies an iteration of the app.
    /// </summary>
    public string? ApplicationVersion { get; set; } // Corresponds to -p:ApplicationVersion

    /// <summary>
    /// Gets or sets the version number of the app.
    /// </summary>
    public string? ApplicationDisplayVersion { get; set; } // Corresponds to -p:ApplicationDisplayVersion

    /// <summary>
    /// Gets or sets a value indicating whether the app should be signed. Default is false.
    /// </summary>
    public bool AndroidKeyStore { get; set; } // Corresponds to -p:AndroidKeyStore, default false

    /// <summary>
    /// Gets or sets a semi-colon delimited property indicating if you want to package the app as an APK file or AAB.
    /// Set to either aab or apk to generate only one format. Default for release builds is aab;apk.
    /// </summary>
    public string? AndroidPackageFormats { get; set; } // Corresponds to -p:AndroidPackageFormats

    /// <summary>
    /// Gets or sets a value indicating whether the app should be trimmed. Default is true for release builds.
    /// </summary>
    public bool PublishTrimmed { get; set; } // Corresponds to -p:PublishTrimmed, default true for release builds

    /// <summary>
    /// Gets or sets the alias for the key in the keystore. This is the keytool -alias value used when creating the keystore.
    /// </summary>
    public string? AndroidSigningKeyAlias { get; set; } // Corresponds to -p:AndroidSigningKeyAlias

    /// <summary>
    /// Gets or sets the password of the key within the keystore file. Supports env: and file: prefixes for security.
    /// </summary>
    public string? AndroidSigningKeyPass { get; set; } // Corresponds to -p:AndroidSigningKeyPass

    /// <summary>
    /// Gets or sets the filename of the keystore file created by keytool.
    /// </summary>
    public string? AndroidSigningKeyStore { get; set; } // Corresponds to -p:AndroidSigningKeyStore

    /// <summary>
    /// Gets or sets the password for the keystore file. Supports env: and file: prefixes for security.
    /// </summary>
    public string? AndroidSigningStorePass { get; set; } // Corresponds to -p:AndroidSigningStorePass
}
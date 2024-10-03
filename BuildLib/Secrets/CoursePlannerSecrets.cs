namespace BuildLib.Secrets;

public interface ICoursePlannerSecrets
{
    string AndroidSigningKeyStore { get; }
    string AndroidSigningKeyAlias { get; }
    string Key { get; }
    GoogleServiceAccount GoogleServiceAccount { get; }
    string KeystoreContents { get; }
    string ApplicationId { get; }
    string GoogleServiceAccountBase64 { get; }
    string KeyUri { get; }
}

public record CoursePlannerSecrets
{
    public string AndroidSigningKeyStore { get; set; } = null!;
    public string AndroidSigningKeyAlias { get; set; } = null!;
    public string Key { get; set; } = null!;
    public GoogleServiceAccount GoogleServiceAccount { get; set; } = null!;
    public string KeystoreContents { get; set; } = null!;
    public string ApplicationId { get; set; } = null!;
    public string GoogleServiceAccountBase64 { get; set; } = null!;
    public string KeyUri { get; set; } = null!;
}
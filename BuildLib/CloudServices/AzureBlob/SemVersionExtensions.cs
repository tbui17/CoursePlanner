using Semver;

namespace BuildLib.CloudServices.AzureBlob;

public static class SemVersionExtensions
{
    public static SemVersion UpdatePatch(this SemVersion version) =>
        new(version.Major, version.Minor, version.Patch + 1);

    public static SemVersion UpdateMinor(this SemVersion version) => new(version.Major, version.Minor + 1, 0);

    public static SemVersion UpdateMajor(this SemVersion version) => new(version.Major + 1, 0, 0);
}
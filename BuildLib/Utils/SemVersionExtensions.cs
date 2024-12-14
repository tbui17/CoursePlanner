using Semver;

namespace BuildLib.Utils;

public static class SemVersionExtensions
{
    public static SemVersion IncrementMajor(this SemVersion version)
    {
        return new SemVersion(version.Major + 1, 0, 0);
    }

    public static SemVersion IncrementMinor(this SemVersion version)
    {
        return new SemVersion(version.Major, version.Minor + 1, 0);
    }

    public static SemVersion IncrementPatch(this SemVersion version)
    {
        return new SemVersion(version.Major, version.Minor, version.Patch + 1);
    }
}
using Semver;

namespace BuildLib.SolutionBuild.Versioning;

public static class SemVersionExtensions
{
    public static bool ContainsNonZeroPositiveValue(this SemVersion version)
    {
        return new[] { version.Major, version.Minor, version.Patch }.Any(x => x > 0);
    }
}
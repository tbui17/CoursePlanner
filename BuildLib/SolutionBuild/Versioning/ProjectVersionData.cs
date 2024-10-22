using Semver;

namespace BuildLib.SolutionBuild.Versioning;

public record ProjectVersionData : IComparable<ProjectVersionData>
{
    public required SemVersion AppVersion { get; init; }
    public required int VersionCode { get; init; }

    public int CompareTo(ProjectVersionData? other)
    {
        if (other is null) return 1;

        var appVersionComparison = ComparisonResult.Create(AppVersion, other.AppVersion);
        var versionCodeComparison = ComparisonResult.Create(VersionCode, other.VersionCode);

        if (!appVersionComparison.SameType(versionCodeComparison))
        {
            var message = new
            {
                appVersionComparison,
                versionCodeComparison,
                Message = "Inconsistent comparison results due to inconsistencies in AppVersion and VersionCode"
            };
            throw new InvalidOperationException(message.ToString());
        }

        return appVersionComparison;
    }

    public static bool operator <(ProjectVersionData left, ProjectVersionData right) =>
        left.CompareTo(right) < 0;

    public static bool operator >(ProjectVersionData left, ProjectVersionData right) => left.CompareTo(right) > 0;
}

public abstract record ComparisonResult
{
    public static ComparisonResult Create(IComparable left, IComparable right)
    {
        return left.CompareTo(right) switch
        {
            < 0 => new LessThanComparisonResult(left, right),
            0 => new EqualComparisonResult(left, right),
            > 0 => new GreaterThanComparisonResult(left, right),
        };
    }

    public static implicit operator int(ComparisonResult result) =>
        result switch
        {
            LessThanComparisonResult => -1,
            EqualComparisonResult => 0,
            GreaterThanComparisonResult => 1,
            _ => throw new InvalidOperationException("Invalid comparison result type")
        };

    public bool SameType(ComparisonResult other) => GetType() == other.GetType();
}

public record LessThanComparisonResult(IComparable Left, IComparable Right) : ComparisonResult;

public record EqualComparisonResult(IComparable Left, IComparable Right) : ComparisonResult;

public record GreaterThanComparisonResult(IComparable Left, IComparable Right) : ComparisonResult;
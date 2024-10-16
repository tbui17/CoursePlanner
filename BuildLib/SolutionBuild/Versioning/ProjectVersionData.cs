using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Riok.Mapperly.Abstractions;
using Semver;

namespace BuildLib.SolutionBuild.Versioning;

public record ProjectVersionData
{
    public required SemVersion AppVersion { get; init; }
    public required int VersionCode { get; init; }
}

public record ValidatedProjectVersionData : ProjectVersionData;

[Mapper]
[Inject(Lifetime = ServiceLifetime.Singleton)]
public partial class ProjectVersionDataMapper
{
    public partial ValidatedProjectVersionData ToValidatedProjectVersionData(ProjectVersionData projectVersionData);
}
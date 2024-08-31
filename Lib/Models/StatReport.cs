using System.Collections.Immutable;

namespace Lib.Models;

public record StatReport
{
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public IReadOnlyDictionary<string, DurationReport> Durations { get; init; } = ImmutableDictionary<string, DurationReport>.Empty;
    public IReadOnlyDictionary<string, int> Counts { get; init; } = ImmutableDictionary<string, int>.Empty;
}
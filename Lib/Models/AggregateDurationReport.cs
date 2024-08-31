using Lib.Interfaces;

namespace Lib.Models;

public record AggregateDurationReport : IDurationReport
{
    public TimeSpan TotalTime { get; init; }
    public TimeSpan RemainingTime { get; init; }
    public TimeSpan CompletedTime { get; init; }
    public TimeSpan AverageDuration { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int RemainingItems { get; init; }
    public double PercentComplete { get; init; }
    public double PercentRemaining { get; init; }
}
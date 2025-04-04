using Lib.Interfaces;

namespace Lib.Models;

public record AggregateDurationReport : IDurationReport
{
    public IReadOnlyList<IGrouping<Type, IDurationReport>> SubReports { get; init; } = [];

    public TimeSpan TotalTime { get; init; }
    public TimeSpan RemainingTime { get; init; }
    public TimeSpan CompletedTime { get; init; }
    public TimeSpan AverageDuration { get; init; }
    public DateTime MinDate { get; init; }
    public DateTime MaxDate { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int RemainingItems => this.RemainingItems();
    public double PercentComplete => this.PercentComplete();
    public double PercentRemaining => this.PercentRemaining();
}
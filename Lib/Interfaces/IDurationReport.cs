namespace Lib.Interfaces;

public interface IDurationReport
{

    TimeSpan TotalTime { get; init; }
    TimeSpan RemainingTime { get; init; }
    TimeSpan CompletedTime { get; init; }
    TimeSpan AverageDuration { get; init; }
    int TotalItems { get; init; }
    int CompletedItems { get; init; }
    int RemainingItems { get; init; }
    double PercentComplete { get; init; }
    double PercentRemaining { get; init; }
}
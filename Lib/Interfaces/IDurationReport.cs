namespace Lib.Interfaces;

public interface IDurationReport
{

    TimeSpan TotalTime { get; }
    TimeSpan CompletedTime { get; }
    TimeSpan RemainingTime { get; }
    TimeSpan AverageDuration { get; }
    DateTime MinDate { get; }
    DateTime MaxDate { get; }
    int TotalItems { get; }
    int CompletedItems { get; }
    int RemainingItems => TotalItems - CompletedItems;
    double PercentComplete => (double) CompletedItems / TotalItems * 100;
    double PercentRemaining => (double) RemainingItems / TotalItems * 100;
}
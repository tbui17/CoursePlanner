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
    int RemainingItems { get; }
    double PercentComplete { get; }
    double PercentRemaining { get; }
}

public static class DurationReportExtensions
{
    public static int RemainingItems(this IDurationReport report) => report.TotalItems - report.CompletedItems;
    public static double PercentComplete(this IDurationReport report) => (double) report.CompletedItems / report.TotalItems * 100;
    public static double PercentRemaining(this IDurationReport report) => (double) report.RemainingItems / report.TotalItems * 100;
}
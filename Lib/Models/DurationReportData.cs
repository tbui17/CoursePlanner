using System.Reflection;
using Lib.Interfaces;

namespace Lib.Models;

public record DurationReportData
{
    public string Title { get; init; } = "";
    public string TimeProgress { get; init; } = "";
    public string TimeRemaining { get; init; } = "";
    public string AverageDuration { get; init; } = "";
    public string MinDate { get; init; } = "";
    public string MaxDate { get; init; } = "";
    public string ItemProgress { get; init; } = "";
    public string ItemRemaining { get; init; } = "";
    public string PercentItemsComplete { get; init; } = "";
    public string PercentItemsRemaining { get; init; } = "";

    public static IEnumerable<PropertyInfo> GetLabelProperties() =>
        typeof(DurationReportData)
            .GetProperties()
            .Where(x => x.Name is not nameof(Title));
}

public static class DurationReportExtensions
{
    public static DurationReportData ToDurationReportData(this IDurationReport report)
    {
        return new DurationReportData
        {
            Title = report switch
            {
                AggregateDurationReport => "Aggregate Report",
                DurationReport x when x.Type != typeof(DurationReport) => $"{x.Type.Name} Report",
                _ => "Report"
            },
            TimeProgress = $"{(int)report.CompletedTime.TotalDays}/{(int)report.TotalTime.TotalDays} days",
            TimeRemaining = $"{(int)report.RemainingTime.TotalDays} days",
            AverageDuration = $"{(int)report.AverageDuration.TotalDays} days",
            MinDate = report.MinDate.ToString("MM/dd/yyyy"),
            MaxDate = report.MaxDate.ToString("MM/dd/yyyy"),
            ItemProgress = $"{report.CompletedItems}/{report.TotalItems}",
            ItemRemaining = $"{report.RemainingItems}",
            PercentItemsComplete = $"{report.PercentComplete}%",
            PercentItemsRemaining = $"{report.PercentRemaining}%"
        };
    }

    public static IEnumerable<DurationReportData> ToDurationReportData(this AggregateDurationReport data)
    {
        return data.SubReports.SelectMany(x => x).Prepend(data).Select(report => report.ToDurationReportData());
    }
}
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

    public static DurationReportData FromDurationReport(IDurationReport report)
    {
        return new DurationReportData
        {
            Title = report switch
            {
                AggregateDurationReport => "Aggregate Report",
                DurationReport x => x.Type.Name + " Report",
                _ => "Report"
            },
            TimeProgress = $"{report.CompletedTime.TotalDays}/{report.TotalTime.TotalDays} days",
            TimeRemaining = $"{report.RemainingTime.TotalDays} days",
            AverageDuration = $"{report.AverageDuration.TotalDays} days",
            MinDate = report.MinDate.ToString("MM/dd/yyyy"),
            MaxDate = report.MaxDate.ToString("MM/dd/yyyy"),
            ItemProgress = $"{report.CompletedItems}/{report.TotalItems}",
            ItemRemaining = $"{report.RemainingItems}",
            PercentItemsComplete = $"{report.PercentComplete}%",
            PercentItemsRemaining = $"{report.PercentRemaining}%"
        };
    }

    public static IEnumerable<DurationReportData> FromDurationReport(AggregateDurationReport data)
    {
        return data.SubReports.SelectMany(x => x).Prepend(data).Select(FromDurationReport);
    }

    public static IEnumerable<PropertyInfo> GetLabelProperties() =>
        typeof(DurationReportData).GetProperties()
            .Where(x => x.Name is not nameof(Title));
}

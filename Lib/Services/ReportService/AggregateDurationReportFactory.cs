using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

public class AggregateDurationReportFactory
{
    public IList<DurationReport> Reports { get; set; } = [];

    private TimeSpan TotalTime() => Reports.SumOrDefault(x => x.TotalTime);
    private TimeSpan CompletedTime() => Reports.SumOrDefault(x => x.CompletedTime);
    private TimeSpan RemainingTime() => Reports.SumOrDefault(x => x.RemainingTime);
    private TimeSpan AverageDuration() => Reports.AverageOrDefault(x => x.AverageDuration);
    private DateTime MinDate() => Reports.MinOrDefault(x => x.MinDate);
    private DateTime MaxDate() => Reports.MaxOrDefault(x => x.MaxDate);
    private int TotalItems() => Reports.SumOrDefault(x => x.TotalItems);
    private int CompletedItems() => Reports.SumOrDefault(x => x.CompletedItems);

    private Dictionary<Type, IDurationReport> SubReports() =>
        Reports.ToDictionary(x => x.Type, x => x as IDurationReport);

    public AggregateDurationReport Create() => new()
    {
        TotalTime = TotalTime(),
        CompletedTime = CompletedTime(),
        RemainingTime = RemainingTime(),
        AverageDuration = AverageDuration(),
        TotalItems = TotalItems(),
        CompletedItems = CompletedItems(),
        MinDate = MinDate(),
        MaxDate = MaxDate(),
        SubReports = SubReports()
    };
}
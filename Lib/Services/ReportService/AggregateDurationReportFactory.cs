using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

public class AggregateDurationReportFactory
{
    public IList<DurationReport> Reports { get; set; } = [];

    private TimeSpan TotalTime() => Reports.Sum(x => x.TotalTime);
    private TimeSpan CompletedTime() => Reports.Sum(x => x.CompletedTime);
    private TimeSpan RemainingTime() => Reports.Sum(x => x.RemainingTime);
    private TimeSpan AverageDuration() => Reports.Average(x => x.AverageDuration);
    private DateTime MinDate() => Reports.Min(x => x.MinDate);
    private DateTime MaxDate() => Reports.Max(x => x.MaxDate);
    private int TotalItems() => Reports.Sum(x => x.TotalItems);
    private int CompletedItems() => Reports.Sum(x => x.CompletedItems);
    private IEnumerable<IGrouping<Type, IDurationReport>> SubReports() => Reports.GroupBy(x => x.Type);

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
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

public class AggregateDurationReportFactory
{
    public IList<DurationReport> Reports { get; set; } = [];
    public DateTime Date { get; set; } = DateTime.Now.Date;

    private TimeSpan TotalTime() => MaxDate() - MinDate();

    private TimeSpan CompletedTime()
    {
        var min = MinDate();
        if (min == default)
        {
            return default;
        }

        return Date.Subtract(min).Max(default);


    }
    private TimeSpan RemainingTime() => (MaxDate() - Date).Clamp(default, TotalTime());
    private TimeSpan AverageDuration() => Reports.AverageOrDefault(x => x.AverageDuration);
    private DateTime MinDate() => Reports.MinOrDefault(x => x.MinDate);
    private DateTime MaxDate() => Reports.MaxOrDefault(x => x.MaxDate);
    private int TotalItems() => Reports.SumOrDefault(x => x.TotalItems);
    private int CompletedItems() => Reports.SumOrDefault(x => x.CompletedItems);

    private List<IGrouping<Type, DurationReport>> SubReports() => Reports.GroupBy(x => x.Type).ToList();

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
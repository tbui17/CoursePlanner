using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
namespace Lib.Services.ReportService;

public class DurationReportFactory
{
    public IList<IDateTimeEntity> Entities { get; set; } = [];
    public DateTime Date { get; set; } = DateTime.Now.Date;

    private int TotalItems() => Entities.Count;

    private int CompletedItems() => Entities.Count(x => x.End <= Date);

    private TimeSpan TotalTime() => MaxDate() - MinDate();

    private TimeSpan CompletedTime() => (Date - MinDate()).Clamp(default, TotalTime());

    private TimeSpan RemainingTime() => MaxDate().Subtract(Date).Max(default);

    private TimeSpan AverageDuration() =>
        Entities.AverageOrDefault(x => x.Duration());

    private Type Type() => Entities.FirstOrDefault()?.GetType() ?? typeof(IDateTimeEntity);

    private DateTime MinDate() => Entities.MinOrDefault(x => x.Start);
    private DateTime MaxDate() => Entities.MaxOrDefault(x => x.End);

    public DurationReport Create() => new()
    {
        TotalTime = TotalTime(),
        CompletedTime = CompletedTime(),
        RemainingTime = RemainingTime(),
        AverageDuration = AverageDuration(),
        TotalItems = TotalItems(),
        CompletedItems = CompletedItems(),
        Type = Type(),
        MinDate = MinDate(),
        MaxDate = MaxDate()
    };
}
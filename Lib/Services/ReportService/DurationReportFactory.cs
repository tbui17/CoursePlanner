using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

internal class DurationReportFactory
{
    public IList<IDateTimeEntity> Entities { get; set; } = [];
    public DateTime Date { get; set; } = DateTime.Now.Date;

    private int TotalItems() => Entities.Count;

    private int CompletedItems() => Entities.Count(x => x.End < Date);

    private TimeSpan TotalTime() => MaxDate() - MinDate();

    private TimeSpan CompletedTime() =>
        BeforeToday().Sum(x => x.Duration());

    private IEnumerable<IDateTimeEntity> BeforeToday() => Entities.Where(x => x.End < Date);

    private TimeSpan RemainingTime() =>
        MaxDate() - Date is var res
        && res > TimeSpan.Zero
            ? res
            : TimeSpan.Zero;

    private TimeSpan AverageDuration() =>
        Entities.Average(x => x.Duration());

    private Type Type() => Entities.FirstOrDefault() switch
    {
        { } x => x.GetType(),
        _ => typeof(IDateTimeEntity)
    };

    private DateTime MinDate() => Entities.Min(x => x.Start);
    private DateTime MaxDate() => Entities.Max(x => x.End);

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
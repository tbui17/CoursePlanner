using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

public class DurationReportFactory
{
    public IList<IDateTimeEntity> Entities { get; set; } = [];
    public DateTime Date { get; set; } = DateTime.Now.Date;

    private int TotalItems() => Entities.Count;

    private int CompletedItems() => Entities.Count(x => x.End < Date);

    private TimeSpan TotalTime() => MaxDate() - MinDate();

    private TimeSpan CompletedTime()
    {
        var beforeToday = BeforeToday();
        return beforeToday.MaxOrDefault(x => x.End)
               -
               beforeToday.MinOrDefault(x => x.Start);
    }

    private List<IDateTimeEntity> BeforeToday() => Entities.Where(x => x.End < Date).ToList();


    private TimeSpan RemainingTime() => new[] { MaxDate() - Date, TimeSpan.Zero }.Max();

    private TimeSpan AverageDuration() =>
        Entities.Count > 0 ? Entities.Average(x => x.Duration()) : TimeSpan.Zero;

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
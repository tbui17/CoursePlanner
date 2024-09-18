using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

public interface IDurationReportFactory : IDurationReport
{
    IReadOnlyCollection<IDateTimeRange> Entities { get; init; }
    DateTime Date { get; init; }
    Type Type { get; }
    DurationReport ToData();
}

public class DurationReportFactory : IDurationReportFactory
{
    public IReadOnlyCollection<IDateTimeRange> Entities { get; init; } = [];
    public DateTime Date { get; init; } = DateTime.Now.Date;
    public Type Type { get; init; } = typeof(IDateTimeRange);


    private DateTime GetDate()
    {
        return Date.Clamp(MinDate, MaxDate);
    }

    private DateTime? _minDate;
    public DateTime MinDate => _minDate ??= Entities.MinOrDefault(x => x.Start);
    private DateTime? _maxDate;
    public DateTime MaxDate => _maxDate ??= Entities.MaxOrDefault(x => x.End);
    private int? _totalItems;
    public int TotalItems => _totalItems ??= Entities.Count;


    private int? _completedItems;
    public int CompletedItems => _completedItems ??= Entities.Count(x => x.End <= GetDate());
    public int RemainingItems => this.RemainingItems();
    public double PercentComplete => this.PercentComplete();
    public double PercentRemaining => this.PercentRemaining();

    private TimeSpan? _totalTime;
    public TimeSpan TotalTime => _totalTime ??= MaxDate - MinDate;

    private TimeSpan? _completedTime;
    public TimeSpan CompletedTime => _completedTime ??= (GetDate() - MinDate).Clamp(default, TotalTime);

    private TimeSpan? _remainingTime;
    public TimeSpan RemainingTime => _remainingTime ??= MaxDate.Subtract(GetDate()).Max(default);

    private TimeSpan? _averageDuration;
    public TimeSpan AverageDuration => _averageDuration ??= Entities.AverageOrDefault(x => x.Duration());


    public DurationReport ToData() => Entities.Count == 0
        ? new()
        : new()
        {
            TotalTime = TotalTime,
            CompletedTime = CompletedTime,
            RemainingTime = RemainingTime,
            AverageDuration = AverageDuration,
            TotalItems = TotalItems,
            CompletedItems = CompletedItems,
            Type = Type,
            MinDate = MinDate,
            MaxDate = MaxDate
        };
}
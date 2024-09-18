using System.Diagnostics;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;

namespace Lib.Services.ReportService;

using static Debug;

public class AggregateDurationReportFactory : IDurationReport
{
    public IReadOnlyCollection<IDurationReportFactory> Reports { get; init; } = [];

    private DateTime? _date;
    private DateTime? _minDate;
    private DateTime? _maxDate;
    private TimeSpan? _totalTime;
    private TimeSpan? _completedTime;
    private TimeSpan? _remainingTime;
    private TimeSpan? _averageDuration;
    private int? _totalItems;
    private int? _completedItems;
    private List<IGrouping<Type, IDurationReportFactory>>? _subReports;


    public DateTime MinDate
    {
        get
        {
            if (_minDate is { } d) return d;
            var res = Reports.MinOrDefault(x => x.MinDate);
            Assert(res >= default(DateTime));
            _minDate = res;
            return res;
        }
    }

    public DateTime MaxDate
    {
        get
        {
            if (_maxDate is { } d) return d;
            var res = Reports.MaxOrDefault(x => x.MaxDate);
            Assert(res >= default(DateTime));
            _maxDate = res;
            return res;
        }
    }


    private DateTime GetDate()
    {
        if (_date is { } d) return d;

        var res = Reports.Select(x => x.Date).Distinct().ToList();
        if (res.Count > 1)
        {
            throw new ArgumentException("Reports must all have the same date.")
            {
                Data = { ["Reports"] = Reports }
            };
        }

        var max = MaxDate;
        var min = MinDate;
        Assert(max >= min);

        var date = res.FirstOrDefault().Clamp(min, max);
        _date = date;
        Assert(date >= min);
        Assert(date <= max);
        return date;
    }

      public TimeSpan TotalTime
    {
        get
        {
            if (_totalTime is { } t) return t;
            var max = MaxDate;
            var min = MinDate;
            Assert(max >= min);
            _totalTime = max - min;
            return _totalTime.Value;
        }
    }


    public TimeSpan CompletedTime
    {
        get
        {
            if (_completedTime is { } t) return t;
            var min = MinDate;
            var date = GetDate();
            Assert(date >= min);
            var res = date.Subtract(min);
            Assert(res >= default(TimeSpan));
            _completedTime = res;
            return res;
        }
    }

    public TimeSpan RemainingTime
    {
        get
        {
            if (_remainingTime is { } t) return t;
            var max = MaxDate;
            var date = GetDate();
            var totalTime = TotalTime;
            Assert(date <= max);
            Assert(totalTime >= default(TimeSpan));
            var res = max.Subtract(date);
            Assert(res >= default(TimeSpan));
            _remainingTime = res;
            return res;
        }
    }


    public TimeSpan AverageDuration
    {
        get
        {
            if (_averageDuration is { } d) return d;
            var duration = Reports.AverageOrDefault(x => x.AverageDuration);
            Assert(duration >= default(TimeSpan));
            _averageDuration = duration;
            return duration;
        }
    }


    public int TotalItems
    {
        get
        {
            if (_totalItems is { } i) return i;
            var sum = Reports.SumOrDefault(x => x.TotalItems);
            Assert(sum >= default(int));
            _totalItems = sum;
            return sum;
        }
    }

    public int CompletedItems
    {
        get
        {
            if (_completedItems is { } i) return i;
            var res = Reports.SumOrDefault(x => x.CompletedItems);
            Assert(res >= default(int));
            _completedItems = res;
            return res;
        }
    }

    public int RemainingItems => this.RemainingItems();
    public double PercentComplete => this.PercentComplete();
    public double PercentRemaining => this.PercentRemaining();

    private IReadOnlyCollection<IGrouping<Type, IDurationReportFactory>> SubReports
    {
        get
        {
            if (_subReports is { } s) return s;
            _subReports = Reports.GroupBy(x => x.Type).ToList();
            return _subReports;
        }
    }


    public AggregateDurationReport Create() => Reports.Count is 0
        ? new()
        : new()
        {
            TotalTime = TotalTime,
            CompletedTime = CompletedTime,
            RemainingTime = RemainingTime,
            AverageDuration = AverageDuration,
            TotalItems = TotalItems,
            CompletedItems = CompletedItems,
            MinDate = MinDate,
            MaxDate = MaxDate,
            SubReports = SubReports.SelectInnerValues(x => x.ToData()).ToArray()
        };
}
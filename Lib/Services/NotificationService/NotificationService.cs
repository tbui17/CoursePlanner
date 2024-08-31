using System.Collections;
using System.Collections.Immutable;
using Lib.Interfaces;
using Lib.Services.MultiDbContext;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services.NotificationService;

public class NotificationService(MultiLocalDbContextFactory dbFactory)
{
    public async Task<IList<INotificationDataResult>> GetUpcomingNotifications()
    {
        var today = DateTime.Now.Date;
        var sameDay = PredicateBuilder.New<INotification>()
            .Start(x => x.Start.Date == today)
            .Or(x => x.End.Date == today);

        await using var db = await dbFactory.CreateAsync<INotification>();

        var res = await db.Query(set => set
            .AsExpandableEFCore()
            .Where(x => x.ShouldNotify)
            .Where(sameDay));


        return res.Select(entity => new NotificationResult
            {
                Entity = entity,
                EndIsUpcoming = x => x.Date == today,
                StartIsUpcoming = x => x.Date == today
            })
            .Cast<INotificationDataResult>()
            .ToList();
    }

    public async Task<IList<INotification>> GetNotificationsForMonth(DateTime date)
    {
        var start = Builder()
            .Start(x => x.Start.Month == date.Month)
            .And(x => x.Start.Year == date.Year);

        var end = Builder()
            .Start(x => x.End.Month == date.Month)
            .And(x => x.End.Year == date.Year);

        var sameMonthAndYear = start.Or(end);

        await using var db = await dbFactory.CreateAsync<INotification>();
        return await db.Query(q => q.AsExpandableEFCore().AsNoTracking().Where(sameMonthAndYear));
    }

    public async Task<IList<INotification>> GetNotificationsWithinDateRange(IDateTimeRange dateRange)
    {
        var inRange = Builder()
            .Start(x => x.Start.Date >= dateRange.Start.Date)
            .And(x => x.End.Date <= dateRange.End.Date);


        await using var db = await dbFactory.CreateAsync<INotification>();
        return await db.Query(q => q.AsExpandableEFCore().AsNoTracking().Where(inRange));
    }

    public async Task<INotification?> GetNextNotificationDate(DateTime date)
    {
        var gtDate = Builder()
            .Start(x => x.Start.Date > date.Date)
            .Or(x => x.End.Date > date.Date);


        await using var db = await dbFactory.CreateAsync<INotification>();
        var res = await db.Query(q => q.AsExpandableEFCore().AsNoTracking().Where(gtDate).Take(1));
        return res.MinBy(x => x.Start);
    }

    public async Task<INotification?> GetPreviousNotificationDate(DateTime date)
    {
        var ltDate = Builder()
            .Start(x => x.Start.Date < date.Date)
            .Or(x => x.End.Date < date.Date);


        await using var db = await dbFactory.CreateAsync<INotification>();
        var res = await db.Query(q => q.AsExpandableEFCore().AsNoTracking().Where(ltDate).Take(1));
        return res.MaxBy(x => x.Start);
    }

    private static ExpressionStarter<INotification> Builder() => PredicateBuilder.New<INotification>();

    public async Task<int> GetTotalItems()
    {
        await using var db = await dbFactory.CreateAsync<INotification>();
        var results = await db.Query(x => x.CountAsync());

        return results.Sum();
    }

    public async Task<INotificationRatio> GetFutureNotifications()
    {
        var today = DateTime.Now.Date;
        var gteToday = Builder()
            .Start(x => x.Start.Date >= today)
            .Or(x => x.End.Date >= today);

        await using var db = await dbFactory.CreateAsync<INotification>();
        var results = await db.Query(q =>
            q.Where(gteToday)
                .GroupBy(x => x.ShouldNotify)
                .Select(x => new { ShouldNotify = x.Key, Count = x.Count() }));

        var active = 0;
        var total = 0;
        foreach (var res in results)
        {
            if (res.ShouldNotify)
            {
                active += res.Count;
            }

            total += res.Count;
        }

        return new NotificationRatio
        {
            Active = active,
            Total = total
        };
    }

    public async Task GetStats()
    {
        var today = DateTime.Now.Date;

    }
}

public interface INotificationRatio
{
    public int Total { get; }
    public int Active { get; }
    public int Inactive => Total - Active;
    public double Percentage => (double)Active / Total * 100;
}

public record NotificationRatio : INotificationRatio
{
    public int Total { get; init; }
    public int Active { get; init; }

    public override string ToString()
    {
        return $"{Active} / {Total}";
    }
}

public record StatReport
{
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public IReadOnlyDictionary<string, DurationReport> Durations { get; init; } = ImmutableDictionary<string, DurationReport>.Empty;
    public IReadOnlyDictionary<string, int> Counts { get; init; } = ImmutableDictionary<string, int>.Empty;
}

public record AggregateDurationReport : IDurationReport
{
    public TimeSpan TotalTime { get; init; }
    public TimeSpan RemainingTime { get; init; }
    public TimeSpan CompletedTime { get; init; }
    public TimeSpan AverageDuration { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int RemainingItems { get; init; }
    public double PercentComplete { get; init; }
    public double PercentRemaining { get; init; }
}

public interface IDurationReport
{

    TimeSpan TotalTime { get; init; }
    TimeSpan RemainingTime { get; init; }
    TimeSpan CompletedTime { get; init; }
    TimeSpan AverageDuration { get; init; }
    int TotalItems { get; init; }
    int CompletedItems { get; init; }
    int RemainingItems { get; init; }
    double PercentComplete { get; init; }
    double PercentRemaining { get; init; }
}

public record DurationReport : IDurationReport
{
    public Type Type { get; init; } = typeof(object);
    public TimeSpan TotalTime { get; init; }
    public TimeSpan RemainingTime { get; init; }
    public TimeSpan CompletedTime { get; init; }
    public TimeSpan AverageDuration { get; init; }
    public int TotalItems { get; init; }
    public int CompletedItems { get; init; }
    public int RemainingItems { get; init; }
    public double PercentComplete { get; init; }
    public double PercentRemaining { get; init; }
}
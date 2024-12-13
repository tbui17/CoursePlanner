using System.Linq.Expressions;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.MultiDbContext;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lib.Services.NotificationService;

public interface INotificationDataService
{
    Task<IList<INotificationDataResult>> GetUpcomingNotifications(IUserSetting settings);
    Task<IList<INotification>> GetNotificationsForMonth(DateTime date);

    Task<IList<INotification>> GetNotificationsWithinDateRange(IDateTimeRange dateRange);

    // Task<INotification?> GetNextNotificationDate(DateTime date);
    // Task<INotification?> GetPreviousNotificationDate(DateTime date);
    Task<int> GetTotalItems();
    // Task<INotificationRatio> GetFutureNotifications();
}

[Inject(typeof(INotificationDataService))]
public class NotificationDataService(
    MultiLocalDbContextFactory dbFactory,
    ILogger<INotificationDataService> logger,
    ITodayProvider todayProvider)
    : INotificationDataService
{
    public async Task<IList<INotificationDataResult>> GetUpcomingNotifications(IUserSetting settings)
    {
        logger.LogInformation("Received request for upcoming notifications. Settings: {Settings}", settings);

        var today = todayProvider.Today();
        var timeAheadDate = today.Add(settings.NotificationRange);
        var dateRange = new DateTimeRange { Start = today, End = timeAheadDate };
        var betweenTodayAndTimeAheadDatePredicate = CreateDateRangePredicate(dateRange);
        betweenTodayAndTimeAheadDatePredicate.And(x => x.ShouldNotify);

        await using var multiDb = await dbFactory.CreateAsync<INotification>();

        var res = await multiDb.Query(set => set
            .AsExpandableEFCore()
            .Where(x => x.ShouldNotify)
            .Where(betweenTodayAndTimeAheadDatePredicate)
        );


        return res
            .Select(INotificationDataResult (entity) => new NotificationResult
                {
                    Entity = entity,
                    EndIsUpcoming = x => x.Date == today,
                    StartIsUpcoming = x => x.Date == today
                }
            )
            .ToList();
    }

    public async Task<IList<INotification>> GetNotificationsForMonth(DateTime date)
    {
        logger.LogInformation("GetNotificationsForMonth: month: {Date}", date);
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
        logger.LogInformation("GetNotificationsWithinDateRange: {DateRange}", dateRange);
        var inRange = CreateDateRangePredicate(dateRange);


        await using var db = await dbFactory.CreateAsync<INotification>();
        return await db.Query(q => q.AsExpandableEFCore().AsNoTracking().Where(inRange));
    }

    public async Task<int> GetTotalItems()
    {
        await using var db = await dbFactory.CreateAsync<INotification>();
        var results = await db.Query(x => x.CountAsync());

        return results.Sum();
    }

    private static Expression<Func<INotification, bool>> CreateDateRangePredicate(IDateTimeRange dateRange)
    {
        return Builder()
            .Start(x => x.Start.Date >= dateRange.Start.Date)
            .And(x => x.Start.Date <= dateRange.End.Date);
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
        return res.MaxBy(x => x.End);
    }

    private static ExpressionStarter<INotification> Builder() => PredicateBuilder.New<INotification>();

    public async Task<INotificationRatio> GetFutureNotifications()
    {
        var today = todayProvider.Today();
        var gteToday = Builder()
            .Start(x => x.Start.Date >= today)
            .Or(x => x.End.Date >= today);

        await using var db = await dbFactory.CreateAsync<INotification>();
        var results = await db.Query(q =>
            q
                .Where(gteToday)
                .GroupBy(x => x.ShouldNotify)
                .Select(x => new { ShouldNotify = x.Key, Count = x.Count() })
        );

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
}
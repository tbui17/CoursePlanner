using System.Linq.Expressions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services;

public class NotificationService(IDbContextFactory<LocalDbCtx> factory)
{
    public async Task<IList<NotificationResult>> GetNotifications()
    {
        await using var db = await factory.CreateDbContextAsync();
        var now = DateTime.Now.Date;
        var queryFactory = new NotificationQueryFactory(now);
        var assessments = queryFactory.CreateUpcomingNotificationQuery(db.Assessments)
           .ToListAsync();
        var courses = queryFactory.CreateUpcomingNotificationQuery(db.Courses)
           .ToListAsync();
        var tasks = await Task.WhenAll(assessments, courses);
        var notifications = tasks
           .SelectMany(x => x)
           .Select(NotificationResult.From);
        return notifications.ToList();
    }

    private class NotificationQueryFactory(DateTime now)
    {
        public IQueryable<INotification> CreateUpcomingNotificationQuery(IQueryable<INotification> queryable) =>
            queryable
               .Where(x => x.ShouldNotify)
               .Where(IsUpcomingExpr<INotification>(now));
    }

    public static Expression<Func<T, bool>> IsUpcomingExpr<T>(DateTime time) where T : INotification
    {
        var oneDay = TimeSpan.FromDays(1);

        return item => item.ShouldNotify &&
                       (
                           (item.Start >= time && item.Start <= time + oneDay) ||
                           (item.End >= time && item.End <= time + oneDay)
                       );
    }


}



public record NotificationResult
{
    public required INotification Entity { get; init; }


    public bool StartIsUpcoming { get; init; }

    public bool EndIsUpcoming { get; init; }

    public bool IsUpcoming => StartIsUpcoming || EndIsUpcoming;

    public string ToMessage()
    {
        if (StartIsUpcoming && EndIsUpcoming)
        {
            return $"{Entity.Name} starts soon at {Entity.Start} and ends soon at {Entity.End}";
        }

        if (StartIsUpcoming)
        {
            return $"{Entity.Name} starts soon at {Entity.Start}";
        }

        if (EndIsUpcoming)
        {
            return $"{Entity.Name} ends soon at {Entity.End}";
        }

        return "";
    }


    public static NotificationResult From(INotification item)
    {
        var now = DateTime.Now;
        return new NotificationResult
        {
            Entity = item, StartIsUpcoming = IsUpcomingImpl(now, item.Start), EndIsUpcoming = IsUpcomingImpl(now, item.End),
        };
    }
    private static bool IsUpcomingImpl(DateTime time, DateTime target)
    {

        var oneDay = TimeSpan.FromDays(1);
        var zero = TimeSpan.Zero;

        var res = target - time;
        return res >= zero && res <= oneDay;
    }


}

public static class NotificationResultExtensions
{
    public static string ToMessage(this IEnumerable<NotificationResult> results) =>
        results
           .Select(x => x.ToMessage())
           .Where(x => !string.IsNullOrEmpty(x))
           .StringJoin(Environment.NewLine);
}
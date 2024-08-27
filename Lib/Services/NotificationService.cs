using System.Linq.Expressions;
using Lib.Interfaces;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services;

using NotificationQuery = Func<IQueryable<INotification>, IQueryable<INotification>>;

public class NotificationService(ILocalDbCtxFactory factory)
{
    public async Task<IList<NotificationResult>> GetUpcomingNotifications()
    {
        await using var db = await factory.CreateDbContextAsync();
        var now = DateTime.Now.Date;
        var queryFactory = new NotificationQueryFactory(now);
        var assessments = await queryFactory.CreateUpcomingNotificationQuery(db.Assessments)
            .ToListAsync();
        var courses = await queryFactory.CreateUpcomingNotificationQuery(db.Courses)
            .ToListAsync();

        var notifications = new[] { assessments, courses }
            .SelectMany(x => x)
            .Select(NotificationResult.From);
        return notifications.ToList();
    }

    private async Task<List<INotification>> GetNotifications(NotificationQuery query)
    {
        await using var db = await factory.CreateDbContextAsync();

        var list = new List<List<INotification>>();


        foreach (var set in db.GetImplementingSets<INotification>())
        {
            var n = await query(set).ToListAsync();
            list.Add(n);
        }

        return list.SelectMany(x => x).ToList();
    }

    public async Task<List<INotification>> GetNotificationsForMonth(DateTime monthDate)
    {
        var res = await GetNotificationsForMonthImpl(monthDate);
        return res;
    }

    private async Task<List<INotification>> GetNotificationsForMonthImpl(DateTime monthDate) =>
        await GetNotifications(q => q
            .Where(x => x.ShouldNotify)
            .Where(x =>
                (x.Start.Month == monthDate.Month && x.Start.Year == monthDate.Year) ||
                x.End.Month == monthDate.Month && x.End.Year == monthDate.Year)
        );



}

file class NotificationQueryFactory(DateTime now)
{
    public IQueryable<INotification> CreateUpcomingNotificationQuery(IQueryable<INotification> queryable) =>
        queryable
            .Where(x => x.ShouldNotify)
            .Where(IsUpcomingExpr<INotification>(now));


    private static Expression<Func<T, bool>> IsUpcomingExpr<T>(DateTime time) where T : INotification
    {
        var oneDay = TimeSpan.FromDays(1);

        return item => item.ShouldNotify &&
                       (
                           (item.Start >= time && item.Start <= time + oneDay) ||
                           (item.End >= time && item.End <= time + oneDay)
                       );
    }
}

public record NotificationResult : INotificationDataResult
{
    private NotificationResult()
    {
    }

    public INotification Entity { get; private init; } = null!;


    public bool StartIsUpcoming { get; private init; }

    public bool EndIsUpcoming { get; private init; }

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
            Entity = item, StartIsUpcoming = IsUpcomingImpl(now, item.Start),
            EndIsUpcoming = IsUpcomingImpl(now, item.End),
        };
    }

    private static bool IsUpcomingImpl(DateTime time, DateTime target)
    {
        return time.Date == target.Date;
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
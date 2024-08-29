using Lib.Interfaces;
using Lib.Utils;

namespace Lib.Services;

using NotificationQuery = Func<IQueryable<INotification>, IQueryable<INotification>>;

public class NotificationService(MultiLocalDbContextFactory factory)
{
    public async Task<IList<NotificationResult>> GetUpcomingNotifications()
    {
        var today = DateTime.Now.Date;

        var res = await GetNotifications(set => set
            .Where(x => x.ShouldNotify)
            .Where(x => x.Start.Date == today || x.End.Date == today));

        return res.Select(x => NotificationResult.From(x, today)).ToList();
    }

    private async Task<IList<INotification>> GetNotifications(NotificationQuery query)
    {
        await using var db = await factory.CreateAsync<INotification>();
        var list = await db.QueryMany(query);
        return list;
    }

    public async Task<IList<INotification>> GetNotificationsForMonth(DateTime monthDate) => await GetNotifications(
        query => query
            .Where(x => x.ShouldNotify)
            .Where(x =>
                (x.Start.Month == monthDate.Month && x.Start.Year == monthDate.Year) ||
                x.End.Month == monthDate.Month && x.End.Year == monthDate.Year)
    );
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


    public static NotificationResult From(INotification item, DateTime time)
    {
        return new NotificationResult
        {
            Entity = item,
            StartIsUpcoming = IsUpcomingImpl(time, item.Start),
            EndIsUpcoming = IsUpcomingImpl(time, item.End),
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
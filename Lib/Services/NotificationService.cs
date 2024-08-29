using System.Linq.Expressions;
using Lib.Interfaces;
using Lib.Utils;
using LinqKit;

namespace Lib.Services;

using NotificationQuery = Func<IQueryable<INotification>, IQueryable<INotification>>;
using NotificationPredicate = Expression<Func<INotification, bool>>;

public class NotificationService(MultiLocalDbContextFactory dbFactory)
{
    public async Task<IList<NotificationResult>> GetUpcomingNotifications()
    {
        var pf = NotificationPredicateFactory.Create(x => x.Date, DateTime.Now.Date);

        var pred = pf.StartEqual().Or(pf.EndEqual());

        var res = await GetNotifications(set => set
            .AsExpandableEFCore()
            .Where(x => x.ShouldNotify)
            .Where(pred));


        return res.Select(x => NotificationResult.From(x, pf)).ToList();
    }

    private async Task<IList<INotification>> GetNotifications(NotificationQuery query)
    {
        await using var db = await dbFactory.CreateAsync<INotification>();
        var list = await db.Query(query);
        return list;
    }

    public async Task<IList<INotification>> GetNotificationsForMonth(DateTime monthDate)
    {
        var monthF = NotificationPredicateFactory.Create(x => x.Month, monthDate);
        var yearF = NotificationPredicateFactory.Create(x => x.Year, monthDate);

        var startPred = monthF.StartEqual().And(yearF.StartEqual());
        var endPred = monthF.EndEqual().And(yearF.EndEqual());

        var pred = startPred.Or(endPred);


        return await GetNotifications(x => x.AsExpandableEFCore().Where(pred));
    }
}

public record NotificationResult : INotificationDataResult
{
    // private NotificationResult()
    // {
    // }

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


    internal static NotificationResult From<T>(INotification item, NotificationPredicateFactory<T> fac)
    {
        return new NotificationResult
        {
            Entity = item,
            StartIsUpcoming = fac.StartEqual().Invoke(item),
            EndIsUpcoming = fac.EndEqual().Invoke(item),
        };
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

internal class NotificationPredicateFactory<T>
{
    private readonly Expression<Func<DateTime, T>> _select;

    internal NotificationPredicateFactory(Expression<Func<DateTime, T>> select)
    {
        _select = select;
    }

    public DateTime Date { get; init; }


    public NotificationPredicate StartEqual() =>
        PredicateBuilder.New<INotification>()
            .Start(x => Equals(_select.Invoke(x.Start), _select.Invoke(Date)));

    public NotificationPredicate EndEqual() => PredicateBuilder.New<INotification>()
        .Start(x => Equals(_select.Invoke(x.End), _select.Invoke(Date)));
}

internal static class NotificationPredicateFactory
{
    internal static NotificationPredicateFactory<T> Create<T>(Expression<Func<DateTime, T>> select, DateTime date)
        where T : struct => new(select) { Date = date };
}
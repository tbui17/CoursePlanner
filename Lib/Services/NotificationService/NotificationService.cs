﻿using Lib.Interfaces;
using Lib.Services.MultiDbContext;
using LinqKit;

namespace Lib.Services.NotificationService;

using NotificationQuery = Func<IQueryable<INotification>, IQueryable<INotification>>;

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
        var monthPf = NotificationPredicateFactory.Create(x => x.Month, monthDate);
        var yearPf = NotificationPredicateFactory.Create(x => x.Year, monthDate);

        var startPred = monthPf.StartEqual().And(yearPf.StartEqual());
        var endPred = monthPf.EndEqual().And(yearPf.EndEqual());

        var pred = startPred.Or(endPred);


        return await GetNotifications(x => x.AsExpandableEFCore().Where(pred));
    }
}
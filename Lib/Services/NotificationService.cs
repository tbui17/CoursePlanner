using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using static Lib.Models.INotification;

namespace Lib.Services;

public class NotificationService(IDbContextFactory<LocalDbCtx> factory)
{
    public async Task<IList<NotificationResult>> GetNotifications()
    {
        await using var db = await factory.CreateDbContextAsync();
        var assessments = await db
           .Assessments
           .Where(x => x.ShouldNotify)
           .ToListAsync();
        var courses = await db
           .Courses
           .Where(x => x.ShouldNotify)
           .ToListAsync();


        var assessmentResults = assessments.Select(NotificationResult.From);
        var courseResults = courses.Select(NotificationResult.From);

        return assessmentResults
           .Concat(courseResults)
           .Where(x => x.IsUpcoming).ToList();
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
            Entity = item, StartIsUpcoming = IsUpcoming(now, item.Start), EndIsUpcoming = IsUpcoming(now, item.End),
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
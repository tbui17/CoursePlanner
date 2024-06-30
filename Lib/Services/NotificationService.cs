using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using static Lib.Models.INotification;

namespace Lib.Services;

public class NotificationService(IDbContextFactory<LocalDbCtx> factory)
{
    public async Task<IEnumerable<NotificationResult>> GetNotifications()
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
           .Where(x => x.IsUpcoming);
    }
}

public record NotificationResult
{
    public required INotification Entity { get; init; }


    public bool StartIsUpcoming { get; init; }

    public bool EndIsUpcoming { get; init; }

    public bool IsUpcoming => StartIsUpcoming || EndIsUpcoming;

    public IList<string> ToMessages()
    {
        var messages = new List<string>();
        if (StartIsUpcoming)
        {
            messages.Add($"{Entity.Name} starts soon at {Entity.Start}");
        }

        if (EndIsUpcoming)
        {
            messages.Add($"{Entity.Name} ends soon at {Entity.End}");
        }

        return messages;
    }


    public static NotificationResult From<T>(T x) where T : INotification
    {
        var now = DateTime.Now;
        return new NotificationResult
        {
            Entity = x, StartIsUpcoming = IsUpcoming(now, x.Start), EndIsUpcoming = IsUpcoming(now, x.End),
        };
    }
}

public static class NotificationResultExtensions
{
    public static string ToMessage(this IEnumerable<NotificationResult> results) =>
        results
           .SelectMany(x => x.ToMessages())
           .StringJoin(Environment.NewLine);
}
using Lib.Interfaces;
using Lib.Utils;

namespace Lib.Services.NotificationService;

public static class NotificationResultExtensions
{
    public static string ToMessage(this IEnumerable<INotificationDataResult> results) =>
        results
            .Select(x => x.ToMessage())
            .Where(x => !string.IsNullOrEmpty(x))
            .StringJoin(Environment.NewLine);
}
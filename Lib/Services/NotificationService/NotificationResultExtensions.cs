using Lib.Utils;

namespace Lib.Services;

public static class NotificationResultExtensions
{
    public static string ToMessage(this IEnumerable<NotificationResult> results) =>
        results
            .Select(x => x.ToMessage())
            .Where(x => !string.IsNullOrEmpty(x))
            .StringJoin(Environment.NewLine);
}
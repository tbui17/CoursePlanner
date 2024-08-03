
using System.Linq.Expressions;

namespace Lib.Models;

public interface INotification
{
    int Id { get; }
    string Name { get; }
    DateTime Start { get; }
    DateTime End { get; }
    bool ShouldNotify { get; }

    public static Expression<Func<T, bool>> IsUpcomingExpr<T>(DateTime time) where T : INotification
    {
        var oneDay = TimeSpan.FromDays(1);

        return item => item.ShouldNotify &&
        (
            (item.Start >= time && item.Start <= time + oneDay) ||
            (item.End >= time && item.End <= time + oneDay)
        );
    }

    public static bool IsUpcoming(DateTime time, DateTime target)
    {
        var oneDay = TimeSpan.FromDays(1);
        var zero = TimeSpan.Zero;

        var res = target - time;
        return res >= zero && res <= oneDay;
    }
}
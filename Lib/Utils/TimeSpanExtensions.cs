namespace Lib.Utils;

public static class TimeSpanExtensions
{
    public static TimeSpan Sum<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector) =>
        source.Sum(x => selector(x).Ticks).Thru(TimeSpan.FromTicks);

    public static TimeSpan Average<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector) =>
        source.Average(x => selector(x).Ticks).Thru(x => (long)x).Thru(TimeSpan.FromTicks);
}
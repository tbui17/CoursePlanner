namespace Lib.Utils;

public static class LinqExtensions
{
    public static TimeSpan SumOrDefault<T>(this ICollection<T> source, Func<T, TimeSpan> selector) =>
        source.Any() ? source.Sum(selector) : default;

    public static int SumOrDefault<T>(this ICollection<T> source, Func<T, int> selector) =>
        source.Any() ? source.Sum(selector) : default;

    public static TimeSpan AverageOrDefault<T>(this ICollection<T> source, Func<T, TimeSpan> selector) =>
        source.Any() ? source.Average(selector) : default;

    public static T2? MinOrDefault<T, T2>(this ICollection<T> source, Func<T, T2> selector) =>
        source.Any() ? source.Min(selector) : default;

    public static T2? MaxOrDefault<T, T2>(this ICollection<T> source, Func<T, T2> selector) =>
        source.Any() ? source.Max(selector) : default;

    public static (T2?, T2?) RangeOrDefault<T, T2>(this ICollection<T> source, Func<T, T2> selector) =>
        (source.MinOrDefault(selector), source.MaxOrDefault(selector));
}
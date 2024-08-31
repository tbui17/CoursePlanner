namespace Lib.Utils;

public static class LinqExtensions
{
    public static TimeSpan SumOrDefault<T>(this IList<T> source, Func<T, TimeSpan> selector) =>
        source.Any() ? source.Sum(selector) : default;
    
    public static int SumOrDefault<T>(this IList<T> source, Func<T, int> selector) =>
        source.Any() ? source.Sum(selector) : default;

    public static TimeSpan AverageOrDefault<T>(this IList<T> source, Func<T, TimeSpan> selector) =>
        source.Any() ? source.Average(selector) : default;
    
    public static T2? MinOrDefault<T,T2>(this IList<T> source, Func<T, T2> selector) =>
        source.Any() ? source.Min(selector) : default;

    public static T2? MaxOrDefault<T,T2>(this IList<T> source, Func<T, T2> selector) =>
        source.Any() ? source.Max(selector) : default;

    
}
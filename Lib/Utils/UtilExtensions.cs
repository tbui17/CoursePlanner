using System.Collections;
using System.Text;

namespace Lib.Utils;

public static class UtilExtensions
{
    public static T GetOrThrow<T>(this ISet<T> set, T value) =>
        set.Contains(value)
            ? value
            : throw new ArgumentException($"Value {value} not found in ${set.StringJoin(",")}");


    public static T? GetOrDefault<T>(this ISet<T> set, T value) =>
        set.Contains(value) ? value : default;

    public static T GetOrDefault<T>(this ISet<T> set, T value, T defaultValue) =>
        set.Contains(value) ? value : defaultValue;


    public static string StringJoin<T>(this IEnumerable<T> collection, string separator = "") =>
        string.Join(separator, collection);

    public static void Times(this int num, Action action)
    {
        for (var i = 0; i < num; i++)
        {
            action();
        }
    }

    public static IEnumerable<T> Times<T>(this int num, Func<int, T> func)
    {
        for (var i = 0; i < num; i++)
        {
            yield return func(i);
        }
    }


    public static void Times(this int num, Action<int> action)
    {
        for (var i = 0; i < num; i++)
        {
            action(i);
        }
    }

    public static string SpaceBetweenUppers(this string text)
    {
        var sb = new StringBuilder();

        foreach (var c in text)
        {
            if (char.IsUpper(c) && sb.Length > 0)
            {
                sb.Append(' ');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    public static IEnumerable<T> Tap<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
        {
            action(item);
            yield return item;
        }
    }

    public static IEnumerable<IGrouping<TKey, TResult>> SelectValues<TKey, TValue, TResult>(
        this IEnumerable<IGrouping<TKey, TValue>> groups,
        Func<(TKey Key, TValue Value), TResult> selector
    ) =>
        from g in groups
        let value = g.Select(x => selector((g.Key, x)))
        select new Grouping<TKey, TResult>(g.Key, value);
}

public static class Grouping
{
    public static Grouping<TKey, TValue> Create<TKey, TValue>(IGrouping<TKey, TValue> grouping) =>
        new(grouping.Key, grouping);
}

public record Grouping<TKey, TValue>(TKey Key, IEnumerable<TValue> Values) : IGrouping<TKey, TValue>
{
    public static Grouping<TKey, TValue> FromIGrouping(IGrouping<TKey, TValue> grouping)
    {
        return new Grouping<TKey, TValue>(grouping.Key, grouping);
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        foreach (var value in Values)
        {
            yield return value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
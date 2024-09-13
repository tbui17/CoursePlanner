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

    public static string PascalCaseToPascalSentence(this string text)
    {
        var sb = new StringBuilder();
        sb.Append(char.ToUpper(text[0]));

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

    public static T2 Thru<T1, T2>(this T1 value, Func<T1, T2> func) => func(value);

    public static T Thru<T>(this T value, Action<T> func)
    {
        func(value);
        return value;
    }

    public static IEnumerable<Grouping<TKey, TResult>> SelectValues<TKey, TValue, TResult>(
        this IEnumerable<IGrouping<TKey, TValue>> groups,
        Func<IGrouping<TKey, TValue>, IEnumerable<TResult>> selector
    ) =>
        from g in groups
        select new Grouping<TKey, TResult>(g.Key, selector(g));

    public static (IList<T>True, IList<T>False) PartitionBy<T>(this IEnumerable<T> collection, Predicate<T> predicate)
    {
        var trueList = new List<T>();
        var falseList = new List<T>();

        foreach (var item in collection)
        {
            if (predicate(item))
            {
                trueList.Add(item);
                continue;
            }

            falseList.Add(item);
        }

        return (trueList, falseList);
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> collection) where T : class =>
        collection.OfType<T>();

    public static Func<T, bool> ToAnyPredicate<T>(this IEnumerable<Func<T, bool>> predicates) =>
        x => predicates.Any(p => p(x));

    public static Func<T,bool> ToAllPredicate<T>(this IEnumerable<Func<T,bool>> predicates) =>
        x => predicates.All(p => p(x));

    public static int DivideRoundedUp(this int dividend, int divisor) =>
        (dividend + divisor - 1) / divisor;
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
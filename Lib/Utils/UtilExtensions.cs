﻿using System.Collections;
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

    public static T2 Thru<T1, T2>(this T1 value, Func<T1, T2> func) => func(value);

    public static T Thru<T>(this T value, Action<T> func)
    {
        func(value);
        return value;
    }

    public static IEnumerable<IGrouping<TKey, TResult>> SelectValues<TKey, TValue, TResult>(
        this IEnumerable<IGrouping<TKey, TValue>> groups,
        Func<IGrouping<TKey, TValue>, IEnumerable<TResult>> selector
    ) =>
        from g in groups
        select new Grouping<TKey, TResult>(g.Key, selector(g));

    public static IEnumerable<IGrouping<TKey, TResult>> SelectInnerValues<TKey, TValue, TResult>(
        this IEnumerable<IGrouping<TKey, TValue>> groups,
        Func<TValue, TResult> selector
    ) =>
        from g in groups
        select new Grouping<TKey, TResult>(g.Key, g.Select(selector));

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

    public static Func<T, bool> ToAllPredicate<T>(this IEnumerable<Func<T, bool>> predicates) =>
        x => predicates.All(p => p(x));

    public static int DivideRoundedUp(this int dividend, int divisor) =>
        (dividend + divisor - 1) / divisor;

    public static IEnumerable<TValue> Values<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> groupings) =>
        groupings.SelectMany(x => x);


    public static int Delete<T>(this IList<T> list, Func<T, bool> predicate)
    {
        var items = list.Where(predicate).ToList();
        var i = 0;
        foreach (var item in items)
        {
            list.Remove(item);
            i++;
        }

        return i;
    }

    public static Lazy<T2> ToLazy<T, T2>(this IEnumerable<T> collection, Func<IEnumerable<T>, T2> selector) =>
        new(() => selector(collection));
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

    public static Grouping<TKey?, TValue> Empty => new(default, Array.Empty<TValue>());

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
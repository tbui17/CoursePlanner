using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BuildLib.Utils;

public static class UtilExtensions
{
    public static T2 Thru<T1, T2>(this T1 t1, Func<T1, T2> func) => func(t1);

    public static T1 Tap<T1>(this T1 t1, Action<T1> func)
    {
        func(t1);
        return t1;
    }

    public static T1 Tap<T1, T2>(this T1 t1, Func<T1, T2> func)
    {
        func(t1);
        return t1;
    }

    public static Dictionary<string, object> ToPropertyDictionary<T>(this T obj) where T : class
    {
        return obj
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead)
            .ToDictionary(x => x.Name, x => x.GetValue(obj))!;
    }

    public static IEnumerable<ObjectNode> GetPropertiesRecursive<T>(this T obj) where T : class
    {
        var root = new ObjectNode { Value = obj, Parent = null, Property = null };
        var queue = new Queue<ObjectNode>([root]);

        while (queue.Count > 0)
        {
            var next = queue.Dequeue();

            var nodes = next
                .Value!
                .GetPublicProperties()
                .Select(x => new ObjectNode
                    {
                        Value = x.GetValue(next.Value),
                        Parent = next,
                        Property = x
                    }
                );

            foreach (var node in nodes)
            {
                if (node.CanRecurse)
                {
                    queue.Enqueue(node);
                    continue;
                }

                yield return node;
            }
        }
    }

    public static PropertyInfo[] GetPublicProperties<T>(this T next) where T : class =>
        next
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);


    public static string ConfigurationKeyName<T>(this T obj, Expression<Func<T, object>> expression) where T : notnull
    {
        var member = expression.Body as MemberExpression;
        var name = member?.Member.Name ?? throw new ArgumentException("Expression must be a member expression.");

        return obj
                   .GetType()
                   .GetProperty(name)
                   ?.GetCustomAttribute<ConfigurationKeyNameAttribute>()
                   ?.Name ??
               throw new ArgumentException("Property must have a ConfigurationKeyNameAttribute.");
    }

    public static StringBuilder AppendSpace(this StringBuilder sb, string value = "") => sb.Append(value).Append(' ');

    public static IEnumerable<KeyValuePair<T, T2>> ToKeyValuePairs<T, T2>(
        this IEnumerable<T> items,
        Func<T, T2> selector
    ) =>
        items.Select(x => new KeyValuePair<T, T2>(x, selector(x)));

    public static IEnumerable<KeyValuePair<T, T3>> SelectValues<T, T2, T3>(
        this IEnumerable<KeyValuePair<T, T2>> items,
        Func<KeyValuePair<T, T2>, T3> selector
    ) =>
        items.Select(x => new KeyValuePair<T, T3>(x.Key, selector(x)));


    public static IEnumerable<KeyValuePair<T3, T2>> SelectKeys<T, T2, T3>(
        this IEnumerable<KeyValuePair<T, T2>> items,
        Func<KeyValuePair<T, T2>, T3> selector
    ) =>
        items.Select(x => new KeyValuePair<T3, T2>(selector(x), x.Value));


    public static IEnumerable<Task<KeyValuePair<T, T3>>> SelectValues<T, T2, T3>(
        this IEnumerable<KeyValuePair<T, T2>> items,
        Func<KeyValuePair<T, T2>, Task<T3>> selector
    ) =>
        items.Select(async x => new KeyValuePair<T, T3>(x.Key, await selector(x)));

    public static string WrapIn(this string value, string wrapper) => $"{wrapper}{value}{wrapper}";
}
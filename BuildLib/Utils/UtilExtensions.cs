using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using LogLevel = Google.Apis.Logging.LogLevel;

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

    public static PropertyInfo[] GetPublicProperties(this Type type) =>
        type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

    public static PropertyInfo[] GetPublicProperties<T>(this T next) where T : class =>
        next
            .GetType()
            .GetPublicProperties();

    public static bool IsObjectType(this Type type) =>
        type is { IsClass: true, IsPrimitive: false } && type != typeof(string);


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

    public static bool ContainsIgnoreCase(this string text, string value) =>
        text.Contains(value, StringComparison.OrdinalIgnoreCase);

    public static bool EqualsIgnoreCase(this string text, string value) =>
        text.Equals(value, StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> collection) where T : class =>
        collection.OfType<T>();

    public static IAsyncEnumerable<T> WhereNotNull<T>(this IAsyncEnumerable<T?> collection) where T : class =>
        collection.OfType<T>();

    public static T[] InArray<T>(this T item) => new[] { item };

    public static IDisposable? MethodScope<T>(this ILogger<T> logger, [CallerMemberName] string? methodName = null)
    {
        return logger.BeginScope(new Dictionary<string, object>
            {
                ["Method"] = methodName ?? ""
            }
        );
    }

    public static T SetData<T>(this T exception, string key, object data) where T : Exception
    {
        exception.Data[key] = data;
        return exception;
    }

    public static T SetData<T>(this T exception, object data) where T : Exception
    {
        exception.Data[nameof(Exception.Data)] = data;
        return exception;
    }

    public static LogEventLevel ToLogEventLevel(this LogLevel logLevel) => logLevel switch
    {
        LogLevel.Debug => LogEventLevel.Debug,
        LogLevel.Error => LogEventLevel.Error,
        LogLevel.Info => LogEventLevel.Information,
        LogLevel.Warning => LogEventLevel.Warning,
        LogLevel.All => LogEventLevel.Verbose,
        LogLevel.None => LogEventLevel.Fatal,
        _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
    };

    public static Microsoft.Extensions.Logging.LogLevel ToLogLevel(this LogLevel logLevel) => logLevel switch
    {
        LogLevel.Debug => Microsoft.Extensions.Logging.LogLevel.Debug,
        LogLevel.Error => Microsoft.Extensions.Logging.LogLevel.Error,
        LogLevel.Info => Microsoft.Extensions.Logging.LogLevel.Information,
        LogLevel.Warning => Microsoft.Extensions.Logging.LogLevel.Warning,
        LogLevel.All => Microsoft.Extensions.Logging.LogLevel.Trace,
        LogLevel.None => Microsoft.Extensions.Logging.LogLevel.None,
        _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
    };

    public static bool IsTrueString(this string? value) => value?.EqualsIgnoreCase("true") ?? false;

    public static IEnumerable<T> InterleaveWith<T>(this IEnumerable<T> items, T item) =>
        items.SelectMany(x => new[] { item, x });
}
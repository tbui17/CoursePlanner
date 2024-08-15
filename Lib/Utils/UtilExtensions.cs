using System.Text;

namespace Lib.Utils;

public static class UtilExtensions
{

    public static T GetOrThrow<T>(this ISet<T> set, T value) =>
        set.Contains(value)
            ? value
            : throw new ArgumentException($"Value {value} not found in ${set.StringJoin(",")}");

    public static string StringJoin<T>(this IEnumerable<T> collection, string separator = "") =>
        string.Join(separator, collection);

    public static void Times(this int num, Action action)
    {
        for (var i = 0; i < num; i++)
        {
            action();
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
}
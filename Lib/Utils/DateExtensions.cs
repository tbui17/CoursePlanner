namespace Lib.Utils;

public static class DateExtensions
{
    public static TimeSpan Min(this TimeSpan target, TimeSpan max) => target > max ? max : target;
    public static TimeSpan Max(this TimeSpan target, TimeSpan min) => target < min ? min : target;
    public static TimeSpan Clamp(this TimeSpan target, TimeSpan min, TimeSpan max) => target.Max(min).Min(max);

    public static DateTime Min(this DateTime target, DateTime max) => target > max ? max : target;
    public static DateTime Max(this DateTime target, DateTime min) => target < min ? min : target;
    public static DateTime Clamp(this DateTime target, DateTime min, DateTime max) => target.Max(min).Min(max);
}
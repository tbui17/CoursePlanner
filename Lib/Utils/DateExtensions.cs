namespace Lib.Utils;

public static class DateExtensions
{
    public static TimeSpan Min(this TimeSpan target, TimeSpan max) => target > max ? max : target;
    public static TimeSpan Max(this TimeSpan target, TimeSpan min) => target < min ? min : target;
    public static TimeSpan Clamp(this TimeSpan target, TimeSpan min, TimeSpan max) => target.Max(min).Min(max);
}
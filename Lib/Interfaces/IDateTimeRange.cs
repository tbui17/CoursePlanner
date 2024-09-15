namespace Lib.Interfaces;

public interface IDateTimeRange
{
    DateTime Start { get; }
    DateTime End { get; }


    public static bool Equals(IDateTimeRange range1, IDateTimeRange range2)
    {
        return range1.Start == range2.Start && range1.End == range2.End;
    }
}

public static class DateTimeRangeExtensions
{
    public static TimeSpan Duration(this IDateTimeRange range)
    {
        return range.End - range.Start;
    }

    public static bool StartEqEnd(this IDateTimeRange range)
    {
        return range.Start == range.End;
    }

    public static bool StartGtEnd(this IDateTimeRange range)
    {
        return range.Start > range.End;
    }

    public static bool StartLtEnd(this IDateTimeRange range)
    {
        return range.Start < range.End;
    }

    public static bool StartGteEnd(this IDateTimeRange range)
    {
        return range.Start >= range.End;
    }

    public static bool StartLteEnd(this IDateTimeRange range)
    {
        return range.Start <= range.End;
    }

    public static bool EndGtStart(this IDateTimeRange range)
    {
        return range.End > range.Start;
    }

    public static bool EndLtStart(this IDateTimeRange range)
    {
        return range.End < range.Start;
    }

    public static bool EndGteStart(this IDateTimeRange range)
    {
        return range.End >= range.Start;
    }

    public static bool EndLteStart(this IDateTimeRange range)
    {
        return range.End <= range.Start;
    }
}
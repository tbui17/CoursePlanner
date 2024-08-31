namespace Lib.Interfaces;

public interface IDateTimeRange
{
    DateTime Start { get; }
    DateTime End { get; }


}

public static class DateTimeRangeExtensions
{

    public static TimeSpan Duration(this IDateTimeRange range)
    {
        return range.End - range.Start;
    }
}
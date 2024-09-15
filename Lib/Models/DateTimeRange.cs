using Lib.Interfaces;

namespace Lib.Models;

public record DateTimeRange : IDateTimeRange
{
    public DateTimeRange(){}

    public DateTimeRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }



    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}
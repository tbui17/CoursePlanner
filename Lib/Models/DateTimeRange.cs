using Lib.Interfaces;

namespace Lib.Models;

public record DateTimeRange : IDateTimeRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
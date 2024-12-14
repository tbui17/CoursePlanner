namespace Lib.Interfaces;

public interface IDateTimeRangeEntity : IEntity, IDateTimeRange
{
    new DateTime Start { get; set; }
    new DateTime End { get; set; }
}
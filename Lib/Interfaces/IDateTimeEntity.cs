namespace Lib.Interfaces;

public interface IDateTimeEntity : IEntity, IDateTimeRange
{
    new DateTime Start { get; set; }
    new DateTime End { get; set; }

}
namespace Lib.Interfaces;

public interface INotification : IEntity, IDateTimeRange
{
    bool ShouldNotify { get; }
}
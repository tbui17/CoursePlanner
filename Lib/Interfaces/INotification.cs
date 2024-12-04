namespace Lib.Interfaces;

public interface INotification : IDateTimeRangeEntity
{
    bool ShouldNotify { get; }
}
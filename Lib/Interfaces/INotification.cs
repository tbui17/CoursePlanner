namespace Lib.Interfaces;

public interface INotification : IDateTimeEntity
{
    bool ShouldNotify { get; }
}
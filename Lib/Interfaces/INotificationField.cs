namespace Lib.Interfaces;

public interface INotificationField : IDateTimeEntity, INotification
{
    new bool ShouldNotify { get; set; }
}
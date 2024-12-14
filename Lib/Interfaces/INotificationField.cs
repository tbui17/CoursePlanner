namespace Lib.Interfaces;

public interface INotificationField : INotification
{
    new bool ShouldNotify { get; set; }
}
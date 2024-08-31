namespace Lib.Interfaces;

public interface INotificationDataResult
{
    INotification Entity { get; }
    string ToMessage();
}
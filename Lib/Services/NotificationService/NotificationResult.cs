using Lib.Interfaces;
using LinqKit;

namespace Lib.Services.NotificationService;

public record NotificationResult : INotificationDataResult
{
    private NotificationResult()
    {
    }

    public INotification Entity { get; private init; } = null!;


    public bool StartIsUpcoming { get; private init; }

    public bool EndIsUpcoming { get; private init; }

    public bool IsUpcoming => StartIsUpcoming || EndIsUpcoming;

    public string ToMessage()
    {
        if (StartIsUpcoming && EndIsUpcoming)
        {
            return $"{Entity.Name} starts soon at {Entity.Start} and ends soon at {Entity.End}";
        }

        if (StartIsUpcoming)
        {
            return $"{Entity.Name} starts soon at {Entity.Start}";
        }

        if (EndIsUpcoming)
        {
            return $"{Entity.Name} ends soon at {Entity.End}";
        }

        return "";
    }


    internal static NotificationResult From<T>(INotification item, NotificationPredicateFactory<T> fac)
    {
        return new NotificationResult
        {
            Entity = item,
            StartIsUpcoming = fac.StartEqual().Invoke(item),
            EndIsUpcoming = fac.EndEqual().Invoke(item),
        };
    }
}
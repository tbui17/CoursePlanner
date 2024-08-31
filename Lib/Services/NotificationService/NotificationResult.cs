using Lib.Interfaces;

namespace Lib.Services.NotificationService;

using DatePredicate = Func<DateTime, bool>;

internal record NotificationResult : INotificationDataResult
{
    public required INotification Entity { get; init; }

    internal required DatePredicate StartIsUpcoming { get; init; }
    internal required DatePredicate EndIsUpcoming { get; init; }

    public string ToMessage() =>
        (StartIsUpcoming(Entity.Start), EndIsUpcoming(Entity.End)) switch
        {
            (true, true) => $"{Entity.Name} starts soon at {Entity.Start} and ends soon at {Entity.End}",
            (true, false) => $"{Entity.Name} starts soon at {Entity.Start}",
            (false, true) => $"{Entity.Name} ends soon at {Entity.End}",
            _ => ""
        };
}
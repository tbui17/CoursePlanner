using Lib.Interfaces;
using Lib.Services;

namespace Lib.Models;

public record NotificationRatio : INotificationRatio
{
    public int Total { get; init; }
    public int Active { get; init; }

    public override string ToString()
    {
        return $"{Active} / {Total}";
    }
}
using Lib.Interfaces;

namespace Lib.Models;

public record NotificationData : INotification
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool ShouldNotify { get; set; }
}
using Lib.Interfaces;
using Lib.Traits;

namespace Lib.Services.NotificationService;

internal record NotificationResult : INotificationDataResult
{
    public required INotification Entity { get; init; }
    public string ToMessage() => NotificationData.From(Entity).ToFriendlyText();
}

file record NotificationData : IDateTimeRangeEntity, IFriendlyText
{
    public string Type { get; set; } = "";
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public static NotificationData From(IDateTimeRangeEntity entity)
    {
        var data = new NotificationData();
        data.Assign(entity);
        data.Type = entity.GetFriendlyType();
        return data;
    }
}
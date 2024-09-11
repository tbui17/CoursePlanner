using Lib.Interfaces;

namespace ViewModels.Services.NotificationDataStreamFactory;

public record PageData
{
    public List<INotification> Data { get; init; } = [];
    public int PageCount { get; init; }
}
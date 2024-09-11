using Lib.Interfaces;

namespace ViewModels.Services;

public record CombinedResult
{
    public List<INotification> Data { get; init; } = [];
    public int PageCount { get; init; }
}
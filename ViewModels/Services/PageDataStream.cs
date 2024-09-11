using Lib.Interfaces;

namespace ViewModels.Services;

public record PageDataStream()
{
    public required IObservable<List<INotification>> Data { get; init; }
    public required IObservable<int> PageCount { get; init; }
    public required IObservable<int> ItemCount { get; init; }
}
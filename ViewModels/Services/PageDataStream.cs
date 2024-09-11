using Lib.Interfaces;

namespace ViewModels.Services;

public record PageDataStream(
    IObservable<List<INotification>> Data,
    IObservable<int> PageCount,
    IObservable<int> ItemCount);
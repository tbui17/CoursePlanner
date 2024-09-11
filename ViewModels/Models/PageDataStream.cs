using Lib.Interfaces;

namespace ViewModels.Models;

public record PageDataStream
{
    public void Deconstruct(out IObservable<List<INotification>> data, out IObservable<int> pageCount, out IObservable<int> itemCount)
    {
        data = Data;
        pageCount = PageCount;
        itemCount = ItemCount;
    }

    public required IObservable<List<INotification>> Data { get; init; }
    public required IObservable<int> PageCount { get; init; }
    public required IObservable<int> ItemCount { get; init; }
}
namespace Lib.Models;

public class DeleteLog
{
    private HashSet<int> Data { get; } = new();

    public bool IsEmpty => Data.Count == 0;

    public IEnumerable<int> Value()
    {
        return Data.Where(x => x is not 0);
    }

    public void Add(int id)
    {
        Data.Add(id);
    }

    public void Clear()
    {
        Data.Clear();
    }
}
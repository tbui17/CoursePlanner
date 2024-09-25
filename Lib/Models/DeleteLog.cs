namespace Lib.Models;

public class DeleteLog
{
    private HashSet<int> Data { get; } = new();

    public bool IsEmpty => Data.Count == 0;

    public IEnumerable<int> Value()
    {
        return Data;
    }

    public void Add(int id)
    {
        if (id is 0)
        {
            return;
        }
        Data.Add(id);
    }

    public void Clear()
    {
        Data.Clear();
    }
}
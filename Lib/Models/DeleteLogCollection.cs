namespace Lib.Models;

public class DeleteLogCollection
{
    private HashSet<int> Data { get; } = new();

    public IEnumerable<int> Value() => Data.Where(x => x is not 0);

    public bool IsEmpty => Data.Count == 0;
    
    public void Add(int id)
    {
        Data.Add(id);
    }

    public void Clear()
    {
        Data.Clear();
    }
}
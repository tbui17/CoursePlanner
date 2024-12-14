namespace Lib.Interfaces;

public interface IDatabaseEntry
{
    public int Id { get; set; }
}

public static class DatabaseEntryExtensions
{
    public static bool IsNew(this IDatabaseEntry entry)
    {
        return entry.Id == 0;
    }
}
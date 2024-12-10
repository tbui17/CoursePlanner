namespace Lib.Interfaces;

public interface IEntity : IDatabaseEntry
{
    public string Name { get; set; }
}
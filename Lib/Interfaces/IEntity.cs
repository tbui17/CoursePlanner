namespace Lib.Interfaces;

public interface IEntity : IDatabaseEntry
{
    string Name { get; set; }
}
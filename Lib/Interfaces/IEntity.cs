namespace Lib.Interfaces;

public interface IEntity : IDatabaseEntry, IFriendlyType
{
    public string Name { get; set; }
}
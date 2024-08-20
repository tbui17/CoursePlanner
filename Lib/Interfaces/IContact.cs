namespace Lib.Interfaces;

public interface IContact : IEntity, IContactField
{
    public new string Name { get; set; }
}
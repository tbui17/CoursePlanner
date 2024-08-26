namespace Lib.Interfaces;

public interface IContact : IEntity, IContactForm
{
    public new string Name { get; set; }
}
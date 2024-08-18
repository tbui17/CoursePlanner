namespace Lib.Interfaces;

public interface IUser : IEntity, IUserField
{
    public new string Name { get; set; }
}
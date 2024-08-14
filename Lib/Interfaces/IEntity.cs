namespace Lib.Interfaces;

public interface IEntity
{
    int Id { get; }
    string Name { get; }


    Exception? ValidateName()
    {
        return string.IsNullOrWhiteSpace(Name)
            ? new ArgumentException("Name cannot be null or empty")
            : null;
    }
}
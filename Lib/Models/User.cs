using Lib.Interfaces;

namespace Lib.Models;

public interface ILogin
{
    public string Username { get; }
    public string Password { get; }
}

public class User : ILogin, IEntity
{
    public int Id { get; set; }

    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    string IEntity.Name
    {
        get => Username;
        set => Username = value;
    }
}
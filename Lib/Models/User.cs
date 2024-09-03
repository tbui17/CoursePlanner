using Lib.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public interface ILogin
{
    public string Username { get; }
    public string Password { get; }
}

[Index(nameof(Username), IsUnique = true)]
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

    public UserSetting CreateUserSetting()
    {
        return new UserSetting()
        {
            UserId = Id,
            User = this,
        };
    }
}
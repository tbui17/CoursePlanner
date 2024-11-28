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
    public byte[] Salt { get; set; } = [];
    public int UserSettingId { get; set; }
    public UserSetting UserSetting { get; set; } = null!;
    public int Id { get; set; }

    string IEntity.Name
    {
        get => Username;
        set => Username = value;
    }

    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
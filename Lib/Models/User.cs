using Lib.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

[Index(nameof(Username), IsUnique = true)]
public class User : ILogin, IEntity, IUserDetail
{
    public byte[] Salt { get; set; } = [];
    public UserSetting? UserSetting { get; set; }
    public int Id { get; set; }

    string IEntity.Name
    {
        get => Username;
        set => Username = value;
    }

    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
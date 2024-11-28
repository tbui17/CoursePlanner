using Lib.Interfaces;

namespace Lib.Models;

public class UserSetting : IUserSetting, IUserSettingForm
{
    public int Id { get; set; }
    public User User { get; set; } = null!;
    public TimeSpan NotificationRange { get; set; }
    public int UserId { get; set; }
}
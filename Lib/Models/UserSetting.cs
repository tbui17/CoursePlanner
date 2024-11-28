using Lib.Interfaces;

namespace Lib.Models;

public class UserSetting : IUserSetting, IUserSettingForm
{
    public static readonly UserSetting DefaultUserSetting = new()
    {
        NotificationRange = TimeSpan.FromDays(60)
    };

    public int Id { get; set; }
    public User User { get; set; } = null!;

    public static IUserSetting DefaultUserSettingValue => DefaultUserSetting;
    public TimeSpan NotificationRange { get; set; }
    public int UserId { get; set; }

    public static UserSetting CreateDefaultUserSetting(int userId) => new()
    {
        UserId = userId,
        NotificationRange = DefaultUserSetting.NotificationRange
    };
}
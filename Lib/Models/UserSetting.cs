namespace Lib.Models;


public class UserSetting
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public TimeSpan NotificationRange { get; set; }

}
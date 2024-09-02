namespace Lib.Models;


public class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public TimeSpan NotificationRange { get; set; }

}
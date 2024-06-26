namespace Lib.Models;

public class Note
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public string Value { get; set; } = string.Empty;
}
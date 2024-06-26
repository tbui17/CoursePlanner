namespace Lib.Models;

public class TermCourse
{
    public int Id { get; set; }
    public int TermId { get; set; }
    public Term Term { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Status Status { get; set; } = Status.PlanToTake;
    public bool NotifyStart { get; set; }
    public bool NotifyEnd { get; set; }
}
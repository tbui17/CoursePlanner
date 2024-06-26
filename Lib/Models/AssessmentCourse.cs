namespace Lib.Models;

public class AssessmentCourse
{
    public int Id { get; set; }
    public int AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int StudentId { get; set; }
    public User Student { get; set; } = null!;
    public bool NotifyStart { get; set; }
    public bool NotifyEnd { get; set; }
}
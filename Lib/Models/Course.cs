namespace Lib.Models;

public class Course
{
    public int Id { get; set; }
    public int InstructorId { get; set; }
    public int PerformanceAssessmentId { get; set; }
    public int ObjectiveAssessmentId { get; set; }

    public User Instructor { get; set; } = null!;
    public PerformanceAssessment PerformanceAssessment { get; set; } = null!;
    public ObjectiveAssessment ObjectiveAssessment { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
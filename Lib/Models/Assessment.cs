namespace Lib.Models;

public class Assessment
{
    public int Id { get; set; }
    public AssessmentType Type { get; set; } = AssessmentType.Performance;
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DateTime.Now;
    public DateTime End { get; set; } = DateTime.Now;
    public bool ShouldNotify { get; set; }

    public int CourseId { get; set; }

    public static Assessment From(Course course)
    {
        return new Assessment
        {
            CourseId = course.Id,
            Name = string.Empty,
            Start = course.Start,
            End = course.End,
            ShouldNotify = false,
        };
    }
}

public enum AssessmentType
{
    Performance,
    Objective
}
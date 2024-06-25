namespace Lib;

public enum Status
{
    None,
    InProgress,
    Completed,
    Dropped,
    PlanToTake,
}

public enum AssessmentType
{
    None,
    Performance,
    Objective,
}

public record Term
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Status Status { get; set; }
}

public record TermCourse
{
    public int Id { get; set; }
    public int TermId { get; set; }
    public int CourseId { get; set; }
}

public record Course
{
    public int Id { get; set; }
    public int InstructorId { get; set; }
    public int PerformanceAssessmentId { get; set; }
    public int ObjectiveAssessmentId { get; set; }
    public string? Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool NotifyStart { get; set; }
    public bool NotifyEnd { get; set; }
}

public record Assessment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public AssessmentType Type { get; set; }
    public string? Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool NotifyStart { get; set; }
    public bool NotifyEnd { get; set; }
}

public record Note
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int UserId { get; set; }
    public string? Value { get; set; }
}

public record User
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
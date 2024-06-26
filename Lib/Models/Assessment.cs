namespace Lib.Models;

public class Assessment
{
    public int Id { get; set; }
    public Course? Course { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    
}

public class PerformanceAssessment : Assessment;

public class ObjectiveAssessment : Assessment;


﻿namespace Lib.Models;

public abstract class AssessmentCourse
{
    public int Id { get; set; }
    public int AssessmentId { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public int StudentId { get; set; }
    public User Student { get; set; } = null!;
    public bool NotifyStart { get; set; }
    public bool NotifyEnd { get; set; }
}

public class PerformanceAssessmentCourse : AssessmentCourse
{
    public PerformanceAssessment Assessment { get; set; } = null!;
}

public class ObjectiveAssessmentCourse : AssessmentCourse
{
    public ObjectiveAssessment Assessment { get; set; } = null!;
}
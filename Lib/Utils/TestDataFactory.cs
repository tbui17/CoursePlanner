using Lib.Models;

namespace Lib.Utils;

public class TestDataFactory
{
    public static ObjectiveAssessment CreateObjectiveAssessment(int i)
    {
        var start = new DateTime(2020, 10, i);
        var end = start.AddDays(5);

        return new ObjectiveAssessment
        {
            Id = i,
            Name = $"Objective Assessment {i}",
            Start = start,
            End = end
        };
    }

    public static PerformanceAssessment CreatePerformanceAssessment(int i)
    {
        var start = new DateTime(2020, 11, i);
        var end = start.AddDays(5);

        return new PerformanceAssessment
        {
            Id = i,
            Name = $"Performance Assessment {i}",
            Start = start,
            End = end
        };
    }

    public static ObjectiveAssessmentCourse CreateObjectiveAssessmentCourse(int i)
    {
        return new ObjectiveAssessmentCourse
        {
            Id = i,
            AssessmentId = i,
            CourseId = i,
            StudentId = (i % 2) + 1,
            NotifyStart = i % 2 == 0,
            NotifyEnd = i % 2 != 0
        };
    }

    public static PerformanceAssessmentCourse CreatePerformanceAssessmentCourse(int i)
    {
        return new PerformanceAssessmentCourse
        {
            Id = i,
            AssessmentId = i,
            CourseId = i,
            StudentId = (i % 2) + 1,
            NotifyStart = i % 2 == 0,
            NotifyEnd = i % 2 != 0
        };
    }

    public static Course CreateCourse(int i)
    {
        return new Course
        {
            Id = i,
            InstructorId = (i % 3) + 3,
            PerformanceAssessmentId = i,
            ObjectiveAssessmentId = i,
            Name = $"Course {i}",
            Start = new DateTime(2020, 8, 1),
            End = new DateTime(2020, 12, 31)
        };
    }

    public static Note CreateNote(int i)
    {
        return new Note
        {
            Id = i,
            CourseId = i,
            UserId = (i % 2) + 1,
            Value = $"This is a note for Course {i}."
        };
    }

    public static TermCourse CreateTermCourse(int i)
    {
        return new TermCourse
        {
            Id = i,
            TermId = (i % 4) + 1,
            CourseId = i,
            StudentId = (i % 2) + 1,
            Status = Status.PlanToTake,
            NotifyStart = i % 2 == 0,
            NotifyEnd = i % 2 != 0
        };
    }
}
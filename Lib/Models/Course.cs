namespace Lib.Models;

public class Course : INotification
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public Status Status { get; set; } = Status.PlanToTake;

    public bool ShouldNotify { get; set; }

    public int TermId { get; set; }
    
    public Term Term { get; set; } = null!;

    public int? InstructorId { get; set; }

    public Instructor? Instructor { get; set; }
    
    public ICollection<Assessment> Assessments { get; set; } = [];
    
    public ICollection<Note> Notes { get; set; } = [];

    public static Course From(Term term)
    {
        return new Course
        {
            TermId = term.Id,
            Start = term.Start,
            End = term.End,
        };
    }
}
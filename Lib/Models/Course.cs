using Lib.Interfaces;
using Lib.Utils;

namespace Lib.Models;

public class Course : INotification
{
    public const string PlanToTake = "Plan to Take";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Dropped = "Dropped";

    public static readonly ISet<string> Statuses = new HashSet<string>
    {
        PlanToTake,
        InProgress,
        Completed,
        Dropped
    };


    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DefaultStart();
    public DateTime End { get; set; } = DefaultEnd();

    private string _status = Statuses.First();

    public string Status
    {
        get => _status;
        set => _status = Statuses.GetOrThrow(value);
    }

    public bool ShouldNotify { get; set; }

    public int TermId { get; set; }

    public Term Term { get; set; } = null!;

    public int? InstructorId { get; set; }

    public Instructor? Instructor { get; set; }

    public ICollection<Assessment> Assessments { get; set; } = [];

    public ICollection<Note> Notes { get; set; } = [];


    public Assessment CreateAssessment()
    {
        return new Assessment
        {
            CourseId = Id,
            Name = string.Empty,
            Start = Start,
            End = End,
            ShouldNotify = false,
        };
    }

    public Note CreateNote()
    {
        return new Note
        {
            CourseId = Id,
            Value = string.Empty,
        };
    }
}
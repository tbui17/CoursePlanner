using Lib.Interfaces;

namespace Lib.Models;

public record ShareNote : IFriendlyText
{

    public ShareNote(){}

    public ShareNote(Note note)
    {
        CourseId = note.Course.Id;
        CourseName = note.Course.Name;
        InstructorName = note.Course.Instructor?.ToString() ?? "";
        CourseStart = note.Course.Start;
        CourseEnd = note.Course.End;
        NoteId = note.Id;
        NoteText = note.Value;
    }

    public int CourseId { get; init; }
    public string CourseName { get; init; } = "";
    public string InstructorName { get; init; } = "";
    public DateTime CourseStart { get; init; }
    public DateTime CourseEnd { get; init; }
    public int NoteId { get; init; }
    public string NoteText { get; init; } = "";

}
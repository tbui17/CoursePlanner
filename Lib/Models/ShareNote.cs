using Lib.Utils;

namespace Lib.Models;

public record ShareNote
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

    public string ToFriendlyText() =>
        GetType()
           .GetProperties()
           .Select(prop =>
                {
                    var key = prop.Name.SpaceBetweenUppers();
                    var value = prop.GetValue(this) switch
                    {
                        DateTime date => date.ToShortDateString(),
                        var x => x
                    };

                    var msg = $"{key}: {value}";

                    return msg;
                }
            )
           .StringJoin("\n");

   
}
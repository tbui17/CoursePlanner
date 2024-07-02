using Lib.Utils;

namespace Lib.Models;

public record ShareNote
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = "";
    public string InstructorName { get; set; } = "";
    public DateTime CourseStart { get; set; }
    public DateTime CourseEnd { get; set; }
    public int NoteId { get; set; }
    public string NoteText { get; set; } = "";

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

    public static explicit operator ShareNote(Note note) =>
        new()
        {
            CourseId = note.Course.Id,
            CourseName = note.Course.Name,
            InstructorName = note.Course.Instructor?.ToString() ?? "",
            CourseStart = note.Course.Start,
            CourseEnd = note.Course.End,
            NoteId = note.Id,
            NoteText = note.Value
        };
}
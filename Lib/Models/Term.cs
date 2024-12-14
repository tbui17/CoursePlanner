using Lib.Interfaces;

namespace Lib.Models;

public class Term : IDateTimeRangeEntity
{
    public ICollection<Course> Courses { get; set; } = [];
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DefaultStart();
    public DateTime End { get; set; } = DefaultEnd();


    public Course CreateCourse()
    {
        return new Course
        {
            TermId = Id, Start = Start, End = End,
        };
    }
}
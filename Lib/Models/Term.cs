using Lib.Interfaces;

namespace Lib.Models;


public class Term : IDateTimeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DefaultStart();
    public DateTime End { get; set; } = DefaultEnd();

    public ICollection<Course> Courses { get; set; } = [];


    public Course CreateCourse()
    {
        return new Course
        {
            TermId = Id, Start = Start, End = End,
        };
    }


}
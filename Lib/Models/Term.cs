using Lib.Interfaces;

namespace Lib.Models;

public class Term : IEntity, IDateTimeRange
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DefaultStart();
    public DateTime End { get; set; } = DefaultEnd();

    public ICollection<Course> Courses { get; set; } = [];




}
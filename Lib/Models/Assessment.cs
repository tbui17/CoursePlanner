using Lib.Interfaces;
using Lib.Utils;

namespace Lib.Models;

public class Assessment : INotification
{
    public const string Performance = "Performance";
    public const string Objective = "Objective";

    public static readonly ISet<string> Types = new HashSet<string> { Performance, Objective };

    public int Id { get; set; }

    private string _type = Types.First();

    public string Type
    {
        get => _type;
        set => _type = Types.GetOrThrow(value);
    }

    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DefaultStart();
    public DateTime End { get; set; } = DefaultEnd();
    public bool ShouldNotify { get; set; }

    public int CourseId { get; set; }

    public static Assessment From(Course course)
    {
        return new Assessment
        {
            CourseId = course.Id,
            Name = string.Empty,
            Start = course.Start,
            End = course.End,
            ShouldNotify = false,
        };
    }
}
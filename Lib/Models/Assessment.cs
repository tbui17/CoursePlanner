using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Traits;
using Lib.Utils;

namespace Lib.Models;

public class Assessment : IAssessmentForm
{
    public const string Performance = "Performance";
    public const string Objective = "Objective";

    public static readonly ISet<string> Types = new HashSet<string> { Objective, Performance };

    public static string DefaultType => Objective;

    public int Id { get; set; }

    private string _type = Types.First();

    public string Type
    {
        get => _type;
        set => _type = Types.GetOrDefault(value, DefaultType);
    }

    public string Name { get; set; } = string.Empty;
    public DateTime Start { get; set; } = DefaultStart();
    public DateTime End { get; set; } = DefaultEnd();
    public bool ShouldNotify { get; set; }

    public int CourseId { get; set; }
}

public static class AssessmentExtensions
{
    public static DomainException? GetUniqueValidationException(this IReadOnlyCollection<Assessment> assessments)
    {
        if (assessments.ValidateUnique().ToList() is { Count: > 0 } exceptions)
        {
            return exceptions
                .Select(x => x.Message)
                .Prepend("The assessments failed to meet the following criteria: \n")
                .StringJoin(Environment.NewLine)
                .Thru(x => new DomainException(x));
        }

        return null;
    }

    public static Assessment ToAssessment(this IAssessmentForm form) => new Assessment().SetFromAssessmentForm(form);
}
using System.Diagnostics;
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

    private string _type = Types.First();

    public static string DefaultType => Objective;

    public int Id { get; set; }

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
    public static DomainException? GetUniqueValidationException(this IReadOnlyCollection<IAssessmentForm> assessments)
    {
        if (assessments.ValidateUnique().ToList() is { Count: > 0 } exceptions)
            return exceptions
                .Select(x => x.Message)
                .Prepend("The assessments failed to meet the following criteria: \n")
                .StringJoin(Environment.NewLine)
                .Thru(x => new DomainException(x));

        return null;
    }

    public static Assessment ToAssessment(this IAssessmentForm form)
    {
        return new Assessment().SetFromAssessmentForm(form);
    }

    public static IEnumerable<DomainException> ValidateUnique(this IReadOnlyCollection<IAssessmentForm> assessments)
    {
        var exceptions = new List<string>();
        foreach (var assessment in assessments)
            if (assessment.ValidateNameAndDates() is { } exc)
                exceptions.Add($"Type: '{assessment.Type}', Name: '{assessment.Name}', Message: {exc.Message}");


        if (assessments.ValidateUnique(x => x.Type) is not null) exceptions.Add("Assessment types must be unique.");

        return exceptions.Select(x => new DomainException(x));
    }

    public static DomainException? ValidateLength(this IReadOnlyCollection<IAssessmentForm> assessments)
    {
        return assessments.Count switch
        {
            > 2 => throw new UnreachableException("There should not be more than 2 assessments."),
            2 => new DomainException("Only 2 assessments allowed per course."),
            _ => null
        };
    }
}
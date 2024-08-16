using Lib.Models;

namespace Lib.Interfaces;

public interface IAssessmentType
{
    string Type { get; set; }
}

public static class AssessmentTypeExtensions
{
    public static string OppositeType(this IAssessmentType item) => item.Type switch
    {
        Assessment.Performance => Assessment.Objective,
        Assessment.Objective => Assessment.Performance,
        _ => throw new ArgumentOutOfRangeException()
    };

    public static void SetOppositeType(this IAssessmentType item)
    {
        item.Type = item.OppositeType();
    }

    public static void EnsureOppositeType(this IAssessmentType item, IAssessmentType other)
    {
        if (item.Type == other.Type)
        {
            item.Type = item.OppositeType();
        }
    }
}
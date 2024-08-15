using Lib.Models;

namespace Lib.Interfaces;

public interface IAssessmentType
{
    string Type { get; set; }

    public string OppositeType => Type switch
    {
        Assessment.Performance => Assessment.Objective,
        Assessment.Objective => Assessment.Performance,
        _ => throw new ArgumentOutOfRangeException()
    };

    public void EnsureOppositeType(IAssessmentType other)
    {
        if (Type == other.Type)
        {
            Type = OppositeType;
        }
    }
}
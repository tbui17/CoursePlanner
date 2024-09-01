namespace Lib.Interfaces;

public interface IAssessmentForm : INotificationField, IAssessmentType
{
    public int CourseId { get; set; }
}
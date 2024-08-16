namespace Lib.Interfaces;

public interface IAssessmentForm : INotification, IAssessmentType
{
    new int Id { get; set; }
    new string Name { get; set; }
    new DateTime Start { get; set; }
    new DateTime End { get; set; }
    new bool ShouldNotify { get; set; }
    public int CourseId { get; set; }
}
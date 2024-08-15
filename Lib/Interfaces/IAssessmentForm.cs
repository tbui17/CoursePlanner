using System.Collections.ObjectModel;

namespace Lib.Interfaces;

public interface IAssessmentForm : INotification, IAssessmentType
{
    new int Id { get; set; }
    new string Name { get; set; }
    new DateTime Start { get; set; }
    new DateTime End { get; set; }
    new bool ShouldNotify { get; set; }
}


public interface IAssessmentAssociatedForm : IAssessmentForm
{
    ObservableCollection<string> AssessmentTypes { get; set; }
}
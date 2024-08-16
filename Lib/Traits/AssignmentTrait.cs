using Lib.Interfaces;

namespace Lib.Traits;

public static class AssignmentTrait
{
    public static T Assign<T, T2>(this T form, T2 otherForm)
        where T : IAssessmentForm where T2 : IAssessmentForm
    {
        form.Id = otherForm.Id;
        form.Name = otherForm.Name;
        form.Start = otherForm.Start;
        form.End = otherForm.End;
        form.ShouldNotify = otherForm.ShouldNotify;
        form.Type = otherForm.Type;
        form.CourseId = otherForm.CourseId;

        return form;
    }
}
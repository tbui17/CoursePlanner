using Lib.Interfaces;
using Lib.Models;

namespace Lib.Traits;

public static class AssignmentTrait
{
    public static T SetFromAssessmentForm<T, T2>(this T self, T2 other)
        where T : IAssessmentForm where T2 : IAssessmentForm
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Start = other.Start;
        self.End = other.End;
        self.ShouldNotify = other.ShouldNotify;
        self.Type = other.Type;
        self.CourseId = other.CourseId;

        return self;
    }

    public static T SetFromCourseField<T, T2>(this T self, T2 other)
        where T : ICourseField where T2 : ICourseField
    {
        self.Status = other.Status;
        self.Start = other.Start;
        self.End = other.End;
        self.Id = other.Id;
        self.Name = other.Name;
        self.ShouldNotify = other.ShouldNotify;

        return self;
    }

    public static T SetFromContact<T, T2>(this T self, T2 other)
        where T : IContact where T2 : IContact
    {
        self.Id = other.Id;
        self.SetFromContactForm(other);

        return self;
    }

    public static T SetFromContactForm<T, T2>(this T self, T2 other)
        where T : IContactForm where T2 : IContactForm
    {
        self.Name = other.Name;
        self.Email = other.Email;
        self.Phone = other.Phone;

        return self;
    }

    public static T SetFromNoteField<T, T2>(this T self, T2 other)
        where T : INoteField where T2 : INoteField
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Value = other.Value;

        return self;
    }


}
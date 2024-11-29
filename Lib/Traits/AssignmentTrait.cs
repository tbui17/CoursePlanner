using Lib.Interfaces;

namespace Lib.Traits;

public static class AssignmentTrait
{
    public static IAssessmentForm Assign(this IAssessmentForm self, IAssessmentForm other)
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

    public static ICourseField Assign(this ICourseField self, ICourseField other)
    {
        self.Status = other.Status;
        self.Start = other.Start;
        self.End = other.End;
        self.Id = other.Id;
        self.Name = other.Name;
        self.ShouldNotify = other.ShouldNotify;

        return self;
    }

    public static IContact Assign(this IContact self, IContact other)
    {
        self.Id = other.Id;
        Assign((IContactForm)self, other);

        return self;
    }

    public static IContactForm Assign(this IContactForm self, IContactForm other)
    {
        self.Name = other.Name;
        self.Email = other.Email;
        self.Phone = other.Phone;

        return self;
    }

    public static INoteField Assign(this INoteField self, INoteField other)
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Value = other.Value;

        return self;
    }

    public static IDateTimeEntity Assign(this IDateTimeEntity self, IDateTimeEntity other)
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Start = other.Start;
        self.End = other.End;

        return self;
    }

    public static IUserSetting Assign(this IUserSetting self, IUserSetting other)
    {
        self.NotificationRange = other.NotificationRange;

        return self;
    }
}
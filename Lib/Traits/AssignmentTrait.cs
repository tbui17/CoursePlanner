using Lib.Interfaces;

namespace Lib.Traits;

public static class AssignmentTrait
{
    public static void Assign(this IAssessmentForm self, IAssessmentForm other)
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Start = other.Start;
        self.End = other.End;
        self.ShouldNotify = other.ShouldNotify;
        self.Type = other.Type;
        self.CourseId = other.CourseId;
    }

    public static void Assign(this ICourseField self, ICourseField other)
    {
        self.Status = other.Status;
        self.Start = other.Start;
        self.End = other.End;
        self.Id = other.Id;
        self.Name = other.Name;
        self.ShouldNotify = other.ShouldNotify;
    }

    public static void Assign(this IContact self, IContact other)
    {
        self.Id = other.Id;
        Assign((IContactForm)self, other);
    }

    public static void Assign(this IContactForm self, IContactForm other)
    {
        self.Name = other.Name;
        self.Email = other.Email;
        self.Phone = other.Phone;
    }

    public static void Assign(this INoteField self, INoteField other)
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Value = other.Value;
    }

    public static void Assign(this IDateTimeRangeEntity self, IDateTimeRangeEntity other)
    {
        self.Id = other.Id;
        self.Name = other.Name;
        self.Start = other.Start;
        self.End = other.End;
    }

    public static void Assign(this IUserSetting self, IUserSetting other)
    {
        self.NotificationRange = other.NotificationRange;
    }
}
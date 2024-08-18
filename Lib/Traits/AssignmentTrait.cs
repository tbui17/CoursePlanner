using Lib.Interfaces;

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

    public static T SetFromUser<T, T2>(this T self, T2 other)
        where T : IUser where T2 : IUser
    {
        self.Id = other.Id;
        self.SetFromUserField(other);

        return self;
    }

    public static T SetFromUserField<T, T2>(this T self, T2 other)
        where T : IUserField where T2 : IUserField
    {
        self.Name = other.Name;
        self.Email = other.Email;
        self.Phone = other.Phone;

        return self;
    }
}
using Lib.Interfaces;

namespace Lib.Utils;

public static class ValidationExtensions
{

    public static Exception? ValidateName(this IEntity entity)
    {
        return string.IsNullOrWhiteSpace(entity.Name)
            ? new ArgumentException("Name cannot be null or empty")
            : null;
    }

    public static Exception? ValidateDates(this IDateTimeRange dateTimeRange)
    {
        return dateTimeRange.Start >= dateTimeRange.End
            ? new ArgumentException("Start must be before end")
            : null;
    }

}
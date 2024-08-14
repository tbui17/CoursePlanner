using Lib.Exceptions;
using Lib.Interfaces;

namespace Lib.Traits;

public static class ValidationExtensions
{

    public static DomainException? ValidateName(this IEntity entity)
    {
        return string.IsNullOrWhiteSpace(entity.Name)
            ? new DomainException("Name cannot be null or empty")
            : null;
    }

    public static DomainException? ValidateDates(this IDateTimeRange dateTimeRange)
    {
        return dateTimeRange.Start >= dateTimeRange.End
            ? new DomainException("Start must be before end")
            : null;
    }

    public static DomainException? ValidateNameAndDates(this IDateTimeEntity entity)
    {
        return AggregateValidation(entity.ValidateName(), entity.ValidateDates());
    }

}
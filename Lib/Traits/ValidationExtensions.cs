using Lib.Exceptions;
using Lib.Interfaces;

namespace Lib.Traits;

public static class ValidationExtensions
{

    public static DomainException? ValidateName<T>(this T entity) where T : IEntity
    {
        return string.IsNullOrWhiteSpace(entity.Name)
            ? new DomainException("Name cannot be null or empty")
            : null;
    }

    public static DomainException? ValidateDates<T>(this T dateTimeRange) where T : IDateTimeRange
    {
        return dateTimeRange.Start >= dateTimeRange.End
            ? new DomainException("Start must be before end")
            : null;
    }

    public static DomainException? ValidateNameAndDates<T>(this T entity) where T : IEntity, IDateTimeRange
    {
        return AggregateValidation(entity.ValidateName(), entity.ValidateDates());
    }

}
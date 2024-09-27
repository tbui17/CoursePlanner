using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Utils;

namespace Lib.Traits;

public static class ValidationTrait
{
    public static DomainException? ValidateName<T>(this T entity) where T : IEntity
    {
        return string.IsNullOrWhiteSpace(entity.Name)
            ? new DomainException("Name cannot be null or empty")
            : null;
    }

    private static DomainException? ValidateDates<T>(this T dateTimeRange) where T : IDateTimeRange
    {
        return dateTimeRange.Start >= dateTimeRange.End
            ? new DomainException("Start must be before end")
            : null;
    }

    public static DomainException? ValidateNameAndDates<T>(this T entity) where T : IEntity, IDateTimeRange
    {
        return AggregateValidation(entity.ValidateName(), entity.ValidateDates());
    }

    public static DomainException? ValidateUnique<T, T2>(this IReadOnlyCollection<T> entities, Func<T, T2> selector)
    {
        return entities.DistinctBy(selector).Count() != entities.Count
            ? new DomainException("Entities must be unique")
            : null;
    }

    private static DomainException? AggregateValidation(params DomainException?[] exceptions)
    {
        var res = exceptions
            .WhereNotNull()
            .Select(x => x.Message)
            .ToList();
        if (res.Count == 0)
        {
            return null;
        }

        return new DomainException(string.Join(Environment.NewLine, res));
    }
}
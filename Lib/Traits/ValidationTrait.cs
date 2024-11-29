using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Utils;

namespace Lib.Traits;

public static class ValidationTrait
{
    public static DomainException? Validate(this IEntity entity)
    {
        return string.IsNullOrWhiteSpace(entity.Name)
            ? new DomainException("Name cannot be null or empty")
            : null;
    }

    private static DomainException? Validate(this IDateTimeRange dateTimeRange)
    {
        return dateTimeRange.Start >= dateTimeRange.End
            ? new DomainException("Start must be before end")
            : null;
    }

    public static DomainException? Validate(this IDateTimeEntity entity)
    {
        return AggregateValidationExceptions(((IEntity)entity).Validate(), ((IDateTimeRange)entity).Validate());
    }

    public static DomainException? ValidateUnique<T, T2>(this IReadOnlyCollection<T> entities, Func<T, T2> selector)
    {
        return entities.DistinctBy(selector).Count() != entities.Count
            ? new DomainException("Entities must be unique")
            : null;
    }

    private static DomainException? AggregateValidationExceptions(params DomainException?[] exceptions)
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
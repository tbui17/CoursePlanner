using Lib.Exceptions;
using Lib.Interfaces;

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

    private static DomainException? AggregateValidation(params DomainException?[] exceptions)
    {
        var res = exceptions.OfType<DomainException>().Select(x => x.Message).ToList();
        if (res.Count == 0)
        {
            return null;
        }

        return new DomainException(string.Join(Environment.NewLine, res));
    }

}
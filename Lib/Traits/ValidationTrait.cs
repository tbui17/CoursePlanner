using Lib.Exceptions;
using Lib.Interfaces;
using Lib.Models;

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

    public static DomainException? ValidateUnique<T,T2>(this IReadOnlyCollection<T> entities, Func<T,T2> selector)
    {
        return entities.DistinctBy(selector).Count() != entities.Count
            ? new DomainException("Entities must be unique")
            : null;
    }

    public static IEnumerable<DomainException> ValidateUnique(this IReadOnlyCollection<Assessment> assessments)
    {
        var exceptions = new List<string>();
        foreach (var assessment in assessments)
        {
            if (assessment.ValidateNameAndDates() is { } exc)
            {
                exceptions.Add($"Type: '{assessment.Type}', Name: '{assessment.Name}', Message: {exc.Message}");
            }
        }


        if (assessments.ValidateUnique(x => x.Type) is not null)
        {
            exceptions.Add("Assessment types must be unique.");
        }

        return exceptions.Select(x => new DomainException(x));
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
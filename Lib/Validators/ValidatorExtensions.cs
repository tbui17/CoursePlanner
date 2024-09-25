using FluentValidation;
using Lib.Exceptions;
using Lib.Utils;

namespace Lib.Validators;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptionsConditions<T, string> Alphanumeric<T>(
        this IRuleBuilder<T, string> builder
    )
    {
        return builder.Custom((x, ctx) =>
            {
                if (!x.All(char.IsLetterOrDigit))
                {
                    ctx.AddFailure($"{ctx.DisplayName} can only contain letters [A-Z], [a-z], and numbers [0-9]");
                }
            }
        );
    }

    public static IRuleBuilderOptionsConditions<T, string> Lower<T>(
        this IRuleBuilder<T, string> builder
    )
    {
        return builder.Custom((x, ctx) =>
            {
                if (!x.All(char.IsLower))
                {
                    ctx.AddFailure($"{ctx.DisplayName} must have lowercase letters.");
                }
            }
        );
    }

    public static Result<T> Check<T>(this IValidator<T> validator, T obj)
    {
        if (validator.GetError(obj) is { } e)
        {
            return e;
        }

        return obj;
    }

    public static Exception? GetError<T>(this IValidator<T> validator, T obj)
    {
        var res = validator.Validate(obj);
        if (res.IsValid) return null;
        return new DomainException(res.ToString());
    }
}
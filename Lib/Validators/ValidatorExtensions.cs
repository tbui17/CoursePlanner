using FluentResults;
using FluentValidation;

namespace Lib.Validators;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptionsConditions<T, string> Alphanumeric<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder.Custom((x, ctx) =>
        {
            if (!x.All(char.IsLetterOrDigit))
            {
                ctx.AddFailure($"{ctx.DisplayName} can only contain letters [A-Z], [a-z], and numbers [0-9]");
            }
        });
    }

    public static IRuleBuilderOptionsConditions<T, string> Lower<T>(
        this IRuleBuilder<T, string> builder)
    {
        return builder.Custom((x, ctx) =>
        {
            if (!x.All(char.IsLower))
            {
                ctx.AddFailure($"{ctx.DisplayName} must have lowercase letters.");
            }
        });
    }

    public static Result<T> Check<T>(this IValidator<T> validator, T obj)
    {
        var res = validator.Validate(obj);
        if (res.IsValid) return obj;
        return Result.Fail(res.Errors.Select(x => x.ErrorMessage));
    }
}
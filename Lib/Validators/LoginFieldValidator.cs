using FluentValidation;
using Lib.Interfaces;

namespace Lib.Validators;

public class LoginFieldValidator : AbstractValidator<ILogin>
{
    public LoginFieldValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(256)
            .Alphanumeric();

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .MaximumLength(256)
            .Alphanumeric();
    }
}
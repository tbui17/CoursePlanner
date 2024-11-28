using FluentValidation;
using Lib.Interfaces;

namespace Lib.Validators;

public class ContactFormValidator : AbstractValidator<IContactForm>
{
    private const string PhoneRegexPattern = @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$";

    public ContactFormValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Phone).NotEmpty().Matches(PhoneRegexPattern);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
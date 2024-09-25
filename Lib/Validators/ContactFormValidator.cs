using System.Text.RegularExpressions;
using FluentValidation;
using Lib.Interfaces;

namespace Lib.Validators;

public partial class ContactFormValidator : AbstractValidator<IContactForm>
{
    private const string PhoneRegexPattern = @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$";

    [GeneratedRegex(PhoneRegexPattern)]
    private static partial Regex PhoneRegex();

    public ContactFormValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Phone).NotEmpty().Matches(PhoneRegexPattern);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
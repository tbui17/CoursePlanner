using FluentValidation;
using Lib.Interfaces;

namespace Lib.Validators;

public class SettingsValidator : AbstractValidator<IUserSetting>
{
    public SettingsValidator()
    {
        RuleFor(x => x.NotificationRange.TotalDays).InclusiveBetween(1, 99999);
    }
}
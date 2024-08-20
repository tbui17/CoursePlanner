using FluentValidation.TestHelper;
using Lib.Models;
using Lib.Validators;

namespace LibTests;


public class LoginFieldValidatorTest
{
    private LoginFieldValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new LoginFieldValidator();
    }

    [Test]
    public void Validate_ValidLoginDetails_NoValidationErrors()
    {
        var model = new LoginDetails("ValidUsername", "ValidPassword123");
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Validate_EmptyUsername_ValidationErrorForUsername()
    {
        var model = new LoginDetails("", "ValidPassword123");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_UsernameTooShort_ValidationErrorForUsername()
    {
        var model = new LoginDetails("ab", "ValidPassword123");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_UsernameTooLong_ValidationErrorForUsername()
    {
        var model = new LoginDetails(new string('a', 257), "ValidPassword123");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_PasswordTooShort_ValidationErrorForPassword()
    {
        var model = new LoginDetails("ValidUsername", "short1");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Validate_PasswordTooLong_ValidationErrorForPassword()
    {
        var model = new LoginDetails("ValidUsername", new string('a', 257));
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Test]
    public void Validate_NonAlphanumericUsername_ValidationErrorForUsername()
    {
        var model = new LoginDetails("Invalid@Username", "ValidPassword123");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Test]
    public void Validate_NonAlphanumericPassword_ValidationErrorForPassword()
    {
        var model = new LoginDetails("ValidUsername", "Invalid@Password");
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

}
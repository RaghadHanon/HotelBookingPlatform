using FluentValidation;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Validators.Extenstions;

/// <summary>
/// Extension methods for validating password properties.
/// </summary>
public static class StrongPasswordExtensions
{
    /// <summary>
    /// Validates a password for complexity requirements.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validation rule is being defined.</param>
    /// <returns>Rule builder options for further chaining validation rules.</returns>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationErrorMessages.RequiredField)
            .MinimumLength(6).WithMessage(ValidationErrorMessages.PasswordTooShort)
            .Matches("[A-Z]").WithMessage(ValidationErrorMessages.PasswordMissingUppercase)
            .Matches("[a-z]").WithMessage(ValidationErrorMessages.PasswordMissingLowercase)
            .Matches("[0-9]").WithMessage(ValidationErrorMessages.PasswordMissingDigits)
            .Matches("[^a-zA-Z0-9]").WithMessage(ValidationErrorMessages.PasswordMissingSpecialCharacter);
    }
}

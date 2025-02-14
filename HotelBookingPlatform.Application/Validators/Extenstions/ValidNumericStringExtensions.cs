using FluentValidation;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Validators.Extenstions;

/// <summary>
/// Extension methods for validating numeric string properties.
/// </summary>
public static class ValidNumericStringExtensions
{
    /// <summary>
    /// Validates if a string is a numeric string of a specific length.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validation rule is being defined.</param>
    /// <param name="length">The exact length the numeric string should be.</param>
    /// <returns>Rule builder options for further chaining validation rules.</returns>
    public static IRuleBuilderOptions<T, string> ValidNumericString<T>(this IRuleBuilder<T, string> ruleBuilder,
        int length)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationErrorMessages.RequiredField)
            .Matches($"^[0-9]{{{length}}}$")
            .WithMessage(ValidationErrorMessages.InvalidNumericStringLength);
    }
}


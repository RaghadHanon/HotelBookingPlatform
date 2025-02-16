using FluentValidation;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Validators.Extenstions;

/// <summary>
/// Extension methods for validating name-related properties.
/// </summary>
public static class ValidNameExtensions
{
    /// <summary>
    /// Validates that a string property contains only letters and has a length within the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder on which the validation rule is being defined.</param>
    /// <param name="minLength">The minimum length of the string. Defaults to 2 characters.</param>
    /// <param name="maxLength">The maximum length of the string. Defaults to 35 characters.</param>
    /// <returns>Rule builder options for further chaining validation rules.</returns>
    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> ruleBuilder,
          int minLength = 2,
          int maxLength = 35)
    {
        return ruleBuilder
            .NotEmpty().WithMessage(ValidationErrorMessages.RequiredField)
            .Matches(@"^[A-Za-z(),\-\s]*$").WithMessage(ValidationErrorMessages.InvalidNameCharacters)
            .Length(minLength, maxLength).WithMessage(ValidationErrorMessages.InvalidNameLength);
    }
}

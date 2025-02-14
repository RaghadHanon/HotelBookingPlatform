using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Extenstions;

namespace HotelBookingPlatform.Application.Validators.Identity;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationErrorMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationErrorMessages.EmailInvalid);

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(ValidationErrorMessages.UsernameRequired)
            .MinimumLength(5).WithMessage(ValidationErrorMessages.UsernameTooShort)
            .MaximumLength(30).WithMessage(ValidationErrorMessages.UsernameTooLong);

        RuleFor(x => x.Password)
            .StrongPassword();

        RuleFor(x => x.FirstName)
            .ValidName(2, 30);

        RuleFor(x => x.LastName)
            .ValidName(2, 30);
    }
}

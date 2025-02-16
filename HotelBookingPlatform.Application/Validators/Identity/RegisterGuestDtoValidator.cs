using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Validators.Extenstions;

namespace HotelBookingPlatform.Application.Validators.Identity;

public class RegisterGuestDtoValidator : AbstractValidator<RegisterGuestDto>
{
    public RegisterGuestDtoValidator()
    {
        Include(new RegisterUserDtoValidator());

        RuleFor(x => x.FirstName)
            .ValidName(2, 30);

        RuleFor(x => x.LastName)
            .ValidName(2, 30);
    }
}


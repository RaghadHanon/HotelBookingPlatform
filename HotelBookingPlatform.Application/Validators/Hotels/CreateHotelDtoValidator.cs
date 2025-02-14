using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Validators.Extenstions;
using static HotelBookingPlatform.Domain.Restrictions.Common;
using static HotelBookingPlatform.Domain.Restrictions.Hotel;
using static HotelBookingPlatform.Domain.Restrictions.Location;

namespace HotelBookingPlatform.Application.Validators.Hotels;

public class CreateHotelDtoValidator : AbstractValidator<CreateHotelDto>
{
    public CreateHotelDtoValidator()
    {
        RuleFor(h => h.Name)
            .ValidName(MinNameLength, MaxNameLength);

        RuleFor(h => h.Owner)
            .ValidName(MinNameLength, MaxNameLength);

        RuleFor(h => h.StarRate)
            .InclusiveBetween(MinHotelStars, MaxHotelStars);

        RuleFor(h => h.Street)
            .NotEmpty();

        RuleFor(h => h.Location.Latitude)
            .NotEmpty()
            .InclusiveBetween(MinLatitude, MaxLatitude);

        RuleFor(h => h.Location.Longitude)
            .NotEmpty()
            .InclusiveBetween(MinLongitude, MaxLongitude);
    }
}

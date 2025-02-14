using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.Validators.Extenstions;
using static HotelBookingPlatform.Domain.Restrictions.Common;

namespace HotelBookingPlatform.Application.Validators.Amenity;
public class CreateAndUpdateAmenityDtoValidator : AbstractValidator<CreateAndUpdateAmenityDto>
{
    public CreateAndUpdateAmenityDtoValidator()
    {
        RuleFor(a => a.Name)
            .ValidName(MinNameLength, MaxNameLength);
    }
}

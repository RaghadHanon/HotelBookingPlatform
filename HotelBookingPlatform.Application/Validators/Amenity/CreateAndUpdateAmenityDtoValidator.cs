using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Amenity;
using static HotelBookingPlatform.Domain.Restrictions.Common;
using HotelBookingPlatform.Application.Validators.Extensions;

namespace HotelBookingPlatform.Application.Validators.Amenity;
public class CreateAndUpdateAmenityDtoValidator : AbstractValidator<CreateAndUpdateAmenityDto>
{
    public CreateAndUpdateAmenityDtoValidator()
    {
        RuleFor(a => a.Name)
            .ValidName(MinNameLength, MaxNameLength);
    }
}

using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Amenity;

public class GetAmenitiesQueryParametersValidator : AbstractValidator<GetAmenitiesQueryParametersDto>
{
    public GetAmenitiesQueryParametersValidator()
    {
        Include(new QueryParametersValidator<AmenitySortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidAmenitySortColumn);
    }
}

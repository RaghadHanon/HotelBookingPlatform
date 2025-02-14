using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Hotels;

public class GetHotelsQueryParametersValidator : AbstractValidator<GetHotelsQueryParametersDto>
{
    public GetHotelsQueryParametersValidator()
    {
        Include(new QueryParametersValidator<HotelSortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidHotelSortColumn);
    }
}

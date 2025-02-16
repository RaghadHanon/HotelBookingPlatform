using FluentValidation;
using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Cities;

public class GetCitiesQueryParametersValidator : AbstractValidator<GetCitiesQueryParametersDto>
{
    public GetCitiesQueryParametersValidator()
    {
        Include(new QueryParametersValidator<CitySortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidCitySortColumn);
    }
}

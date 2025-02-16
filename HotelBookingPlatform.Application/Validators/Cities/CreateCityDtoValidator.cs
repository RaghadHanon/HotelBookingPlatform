using FluentValidation;
using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.Validators.Extenstions;
using static HotelBookingPlatform.Domain.Restrictions.City;
using static HotelBookingPlatform.Domain.Restrictions.Common;

namespace HotelBookingPlatform.Application.Validators.Cities;

public class CreateCityDtoValidator : AbstractValidator<CreateCityDto>
{
    public CreateCityDtoValidator()
    {
        RuleFor(c => c.Name)
            .ValidName(MinNameLength, MaxNameLength);

        RuleFor(c => c.Country)
            .ValidName(MinNameLength, MaxNameLength);

        RuleFor(c => c.PostOffice)
            .ValidNumericString(PostOfficeLength);
    }

}

using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Bookings;

public class GetBookingsQueryParametersValidator : AbstractValidator<GetBookingsQueryParametersDto>
{
    public GetBookingsQueryParametersValidator()
    {
        Include(new QueryParametersValidator<BookingSortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidBookingSortColumn);
    }
}

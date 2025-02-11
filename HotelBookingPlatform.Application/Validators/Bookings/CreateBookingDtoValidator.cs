using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using static HotelBookingPlatform.Domain.Restrictions.Room;

namespace HotelBookingPlatform.Application.Validators.Bookings;

public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingDtoValidator()
    {
        RuleFor(b => b.RoomIds)
            .NotEmpty()
            .WithMessage(ValidationErrorMessages.RoomIdsRequired)
            .ForEach(b => b.NotEmpty().WithMessage(ValidationErrorMessages.RoomIdsRequired));

        RuleFor(b => b.NumberOfAdults)
            .InclusiveBetween(0, MaxRoomCapacity)
            .WithMessage(ValidationErrorMessages.InvalidNumberOfAdults);

        RuleFor(b => b.NumberOfChildren)
            .InclusiveBetween(0, MaxRoomCapacity)
            .WithMessage(ValidationErrorMessages.InvalidNumberOfChildren);

        RuleFor(b => b.CheckInDate)
            .NotEmpty()
            .WithMessage(ValidationErrorMessages.CheckInDateRequired)
            .Must(dt => DateOnly.FromDateTime(dt) >= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage(ValidationErrorMessages.CheckInDateMustBeFuture);

        RuleFor(b => b.CheckOutDate)
            .NotEmpty()
            .WithMessage(ValidationErrorMessages.CheckOutDateRequired)
            .GreaterThanOrEqualTo(b => b.CheckInDate)
            .WithMessage(ValidationErrorMessages.CheckOutDateAfterCheckIn);
    }
}

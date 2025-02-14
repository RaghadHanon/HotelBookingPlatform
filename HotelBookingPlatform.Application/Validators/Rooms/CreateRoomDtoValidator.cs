using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using static HotelBookingPlatform.Domain.Restrictions.Room;

namespace HotelBookingPlatform.Application.Validators.Rooms;

public class CreateRoomDtoValidator : AbstractValidator<CreateRoomDto>
{
    public CreateRoomDtoValidator()
    {
        RuleFor(r => r.ChildrenCapacity)
            .InclusiveBetween(MinRoomCapacity, MaxRoomCapacity);

        RuleFor(r => r.AdultsCapacity)
            .InclusiveBetween(MinRoomCapacity, MaxRoomCapacity);

        RuleFor(r => r.Price)
            .InclusiveBetween(MinRoomPrice, MaxRoomPrice);

        RuleFor(r => r.RoomNumber)
            .InclusiveBetween(MinRoomNumber, MaxRoomNumber);

        RuleFor(r => r.RoomType)
            .IsInEnum().WithMessage(ValidationErrorMessages.ProvidedRoomTypeNotValid);
    }
}

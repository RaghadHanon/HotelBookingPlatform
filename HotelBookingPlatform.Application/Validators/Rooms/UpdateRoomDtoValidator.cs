using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Room;
using static HotelBookingPlatform.Domain.Restrictions.Room;

namespace HotelBookingPlatform.Application.Validators.Rooms;

public class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDto>
{
    public UpdateRoomDtoValidator()
    {
        RuleFor(r => r.ChildrenCapacity)
            .InclusiveBetween(MinRoomCapacity, MaxRoomCapacity);

        RuleFor(r => r.AdultsCapacity)
            .InclusiveBetween(MinRoomCapacity, MaxRoomCapacity);

        RuleFor(r => r.RoomNumber)
            .InclusiveBetween(MinRoomNumber, MaxRoomNumber);
    }
}

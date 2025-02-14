using FluentValidation;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Enums.SortingColumns;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Common;

namespace HotelBookingPlatform.Application.Validators.Rooms;

public class GetRoomsQueryParametersValidator : AbstractValidator<GetRoomsQueryParametersDto>
{
    public GetRoomsQueryParametersValidator()
    {
        Include(new QueryParametersValidator<RoomSortColumn>());

        RuleFor(x => x.SortColumn)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.InvalidRoomSortColumn);
    }
}

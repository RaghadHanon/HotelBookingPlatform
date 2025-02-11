using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class UnavailableRoomException : CustomException
{
    public UnavailableRoomException(Guid roomId, DateOnly startDate, DateOnly endDate)
        : base(string.Format(ExceptionsErrorMessages.UnavailableRoom, roomId, startDate, endDate))
    { }
}

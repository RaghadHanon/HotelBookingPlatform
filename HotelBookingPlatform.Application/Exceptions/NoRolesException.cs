using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class NoRolesException : ServerErrorException
{
    public NoRolesException(string userId) :
        base(string.Format(ExceptionsErrorMessages.NoRolesForUser, userId))
    {
    }
}

using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string userId, string role) :
        base(string.Format(ExceptionsErrorMessages.UnauthorizedRoleAccess, userId, role))
    {
    }

    public UnauthorizedException(string userId, Guid guestId) :
        base(string.Format(ExceptionsErrorMessages.UnauthorizedGuestAccess, userId, guestId))
    {
    }

    public UnauthorizedException(string userId, string entityName, object entityId) :
        base(string.Format(ExceptionsErrorMessages.UnauthorizedEntityAccess, userId, entityName, entityId))
    {
    }
}

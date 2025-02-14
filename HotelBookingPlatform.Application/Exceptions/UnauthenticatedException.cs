using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class UnauthenticatedException : Exception
{
    public UnauthenticatedException(string message) : base(message)
    {
    }

    public UnauthenticatedException() : base(ExceptionsErrorMessages.UserIsUnauthenticated)
    {
    }
}

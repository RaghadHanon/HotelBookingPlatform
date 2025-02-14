using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class InvalidUserCredentialsException : Exception
{
    public InvalidUserCredentialsException()
        : base(ExceptionsErrorMessages.InvalidEmailOrPassword)
    {
    }
}

using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class InvalidNumberOfGuestsException : CustomException
{
    public InvalidNumberOfGuestsException(int numberOfAdults, int numberOfChildren)
            : base(string.Format(ExceptionsErrorMessages.InvalidNumberOfGuests, numberOfAdults, numberOfChildren))
    { }

    public InvalidNumberOfGuestsException(string message) : base(message)
    { }
}

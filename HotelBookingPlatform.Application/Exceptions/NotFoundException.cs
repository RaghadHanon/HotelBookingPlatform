using HotelBookingPlatform.Application.Utilities.ErrorMessages;

namespace HotelBookingPlatform.Application.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string name, object key)
        : base(string.Format(ExceptionsErrorMessages.EntityWithKeyNotFound, name, key))
    { }
    public NotFoundException(string name)
        : base(string.Format(ExceptionsErrorMessages.EntityNotFound, name))
    { }
}

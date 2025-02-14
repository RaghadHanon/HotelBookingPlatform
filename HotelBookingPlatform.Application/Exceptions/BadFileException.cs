namespace HotelBookingPlatform.Application.Exceptions;

public class BadFileException : Exception
{
    public BadFileException(string? message) : base(message)
    {
    }
}

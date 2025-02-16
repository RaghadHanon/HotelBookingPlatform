namespace HotelBookingPlatform.Application.Exceptions;

public class ServerErrorException : CustomException
{
    public ServerErrorException(string message) : base(message)
    {
    }
}

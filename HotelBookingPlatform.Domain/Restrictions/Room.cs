namespace HotelBookingPlatform.Domain.Restrictions;

public class Room
{
    public const int MinRoomNumber = 1;
    public const int MaxRoomNumber = 10_000;
    public const int MaxRoomCapacity = 20;
    public const int MinRoomCapacity = 0;
    public const decimal MinRoomPrice = 1;
    public const decimal MaxRoomPrice = 100_000;
}
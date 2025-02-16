namespace HotelBookingPlatform.Domain.Models;

public class Booking : Entity
{
    public Booking()
    {
        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public Booking(Hotel hotel, Guest guest, List<BookingRoom> bookingRooms)
    {
        Id = Guid.NewGuid();
        Hotel = hotel;
        Guest = guest;
        BookingRooms = bookingRooms;
        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public decimal Price { get; set; }
    public int NumberOfAdults { get; set; } = DefaultNumberOfAdults;
    public int NumberOfChildren { get; set; } = DefaultNumberOfChildren;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }

    public Hotel Hotel { get; set; } = default!;
    public Guid HotelId { get; set; }
    public Guest Guest { get; set; } = default!;
    public Guid GuestId { get; set; }
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();

    public readonly static int DefaultNumberOfAdults = 2;
    public readonly static int DefaultNumberOfChildren = 0;
}

namespace HotelBookingPlatform.Domain.Models;

public class BookingRoom : Entity
{
    public BookingRoom()
    {
    }

    public BookingRoom(Booking booking, Room room, Discount discount)
    {
        Id = Guid.NewGuid();
        Booking = booking;
        Room = room;
        Discount = discount;
        BookingId = booking.Id;
        RoomId = room.Id;
        DiscountId = discount?.Id;
        CreationDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = default!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!;

    public Guid? DiscountId { get; set; }
    public Discount? Discount { get; set; }

    public decimal FinalPrice
    {
        get
        {
            if (Discount != null)
            {
                return Discount.DiscountedPrice;
            }

            return Room.Price;
        }
        set
        {
            FinalPrice = value;
        }
    }
}

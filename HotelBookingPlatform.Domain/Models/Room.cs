using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Domain.Models;

public class Room : Entity
{
    public int RoomNumber { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int AdultsCapacity { get; set; } = DefaultAdultsCapacity;
    public int ChildrenCapacity { get; set; } = DefaultChildrenCapacity;
    public RoomType RoomType { get; set; }

    public Hotel Hotel { get; set; } = default!;
    public Guid HotelId { get; set; }
    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();

    public readonly static int DefaultAdultsCapacity = 2;
    public readonly static int DefaultChildrenCapacity = 0;
}

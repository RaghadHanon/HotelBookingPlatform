namespace HotelBookingPlatform.Domain.Models;
public class Hotel : Entity
{
    public string Name { get; set; } = default!;
    public string? Street { get; set; } = default!;
    public string Owner { get; set; } = default!;
    public short StarRate { get; set; }
    public string? Description { get; set; }
    public TimeOnly CheckInTime { get; set; } = DefaultCheckInTime;
    public TimeOnly CheckOutTime { get; set; } = DefaultCheckOutTime;

    public City City { get; set; } = default!;
    public Guid CityId { get; set; }
    public Location Location { get; set; } = default!;
    public Guid LocationId { get; set; }
    public ICollection<HotelImage> Images { get; set; } = new List<HotelImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public int RoomsNumber => Rooms.Count;
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();

    public readonly static TimeOnly DefaultCheckInTime = new(14, 0);
    public readonly static TimeOnly DefaultCheckOutTime = new(11, 0);
}


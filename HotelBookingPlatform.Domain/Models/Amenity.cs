namespace HotelBookingPlatform.Domain.Models;

public class Amenity : Entity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; } = default!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}


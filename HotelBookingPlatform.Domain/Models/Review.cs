namespace HotelBookingPlatform.Domain.Models;
public class Review : Entity
{
    public string? Title { get; set; }
    public int Rating { get; set; }
    public string Description { get; set; } = default!;

    public Guest Guest { get; set; } = default!;
    public Guid GuestId { get; set; }
    public Hotel Hotel { get; set; } = default!;
    public Guid HotelId { get; set; }
}

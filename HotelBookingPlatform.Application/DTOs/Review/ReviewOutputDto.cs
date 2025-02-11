using HotelBookingPlatform.Application.DTOs.Guest;

namespace HotelBookingPlatform.Application.DTOs.Review;

public class ReviewOutputDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string Description { get; set; } = default!;
    public int Rating { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModified { get; set; }
    public GuestOutputDto Guest { get; set; } = default!;
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = default!;
}

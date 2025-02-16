namespace HotelBookingPlatform.Domain.Models;
public abstract class Image : Entity
{
    public string ImageUrl { get; set; } = default!;
    public string? AlternativeText { get; set; }
}

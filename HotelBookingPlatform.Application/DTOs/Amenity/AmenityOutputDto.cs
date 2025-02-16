namespace HotelBookingPlatform.Application.DTOs.Amenity;

public class AmenityOutputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; } = default!;
}
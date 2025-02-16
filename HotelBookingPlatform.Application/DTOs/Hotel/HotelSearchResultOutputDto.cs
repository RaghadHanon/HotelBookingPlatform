using HotelBookingPlatform.Application.DTOs.Location;

namespace HotelBookingPlatform.Application.DTOs.Hotel;

public class HotelSearchResultOutputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int StarRate { get; set; }
    public HotelImageOutputDto HotelImage { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public LocationDto Location { get; set; } = default!;
    public string? Street { get; set; } = default!;
}

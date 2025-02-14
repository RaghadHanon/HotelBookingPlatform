using HotelBookingPlatform.Application.DTOs.Location;

namespace HotelBookingPlatform.Application.DTOs.Hotel;

/// <summary>
/// This class is a DTO for the <see cref="Domain.Models.Hotel"/> entity. Mainly for Recently Visited Hotels feature.
/// </summary>
public class RecentlyVisitedHotelOutputDto
{
    public string HotelName { get; set; } = default!;
    public HotelImageOutputDto HotelImage { get; set; } = default!;
    public string CityName { get; set; } = default!;
    public int StarRating { get; set; }
    public decimal Price { get; set; }
    public LocationDto Location { get; set; } = default!;
    public string? Street { get; set; } = default!;
}

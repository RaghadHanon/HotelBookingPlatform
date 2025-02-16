namespace HotelBookingPlatform.Application.DTOs.City;

/// <summary>
/// This class is a DTO for the <see cref="Domain.Models.City"/> entity. Mainly for Trending Destinations Feature.
/// </summary>
public class CityAsTrendingDestinationOutputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Country { get; set; } = default!;
    public CityImageOutputDto CityImage { get; set; } = default!;
}

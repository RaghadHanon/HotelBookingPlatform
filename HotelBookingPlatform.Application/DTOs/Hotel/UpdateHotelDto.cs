using HotelBookingPlatform.Application.DTOs.Location;

namespace HotelBookingPlatform.Application.DTOs.Hotel;
/// <summary>
/// DTO for creating a hotel
/// </summary>
public class UpdateHotelDto
{
    /// <summary>
    /// Id of the city where the hotel is located
    /// </summary>
    public Guid CityId { get; set; }

    /// <summary>
    /// Name of the hotel
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Owner of the hotel
    /// </summary>>
    public string Owner { get; set; } = default!;

    /// <summary>
    /// Street of the hotel
    /// </summary>
    public string? Street { get; set; } = default!;

    /// <summary>
    /// Location for the hotel  
    /// </summary>
    public CreateLocationDto Location { get; set; } = default!;
}

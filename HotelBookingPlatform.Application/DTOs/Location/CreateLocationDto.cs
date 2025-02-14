namespace HotelBookingPlatform.Application.DTOs.Location;

/// <summary>
/// DTO for creating a location
/// </summary>
public class CreateLocationDto
{
    /// <summary>
    /// Longitude of the location
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Latitude of the location
    /// </summary>
    public double Latitude { get; set; }
}

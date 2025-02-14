namespace HotelBookingPlatform.Application.DTOs.City;

/// <summary>
/// DTO for Creating a City 
/// </summary>
public class CreateCityDto
{
    /// <summary>
    /// Name of the City
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Country of the City
    /// </summary>
    public string Country { get; set; } = default!;

    /// <summary>
    /// Post Office of the City
    /// </summary>
    public string PostOffice { get; set; } = default!;
}

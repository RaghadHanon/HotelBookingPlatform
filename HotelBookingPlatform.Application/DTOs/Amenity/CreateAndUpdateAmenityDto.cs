namespace HotelBookingPlatform.Application.DTOs.Amenity;

/// <summary>
/// DTO for creating and updating an Amenity.
/// </summary>
public class CreateAndUpdateAmenityDto
{
    /// <summary>
    /// Name of the amenity.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description of the amenity.
    /// </summary>
    public string? Description { get; set; } = default!;
}
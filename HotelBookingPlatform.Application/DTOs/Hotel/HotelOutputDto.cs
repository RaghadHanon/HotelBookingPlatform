using HotelBookingPlatform.Application.DTOs.Location;

namespace HotelBookingPlatform.Application.DTOs.Hotel;

/// <summary>
/// This class is a data transfer object (DTO) for the <see cref="Domain.Models.Hotel"/> entity. Mainly for Admin Page.
/// </summary>
public class HotelOutputDto
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModified { get; set; }
    public string Name { get; set; } = default!;
    public string Owner { get; set; } = default!;
    public short StarRate { get; set; }
    public int RoomsNumber { get; set; }
    public LocationDto Location { get; set; } = default!;
    public string? Street { get; set; } = default!;
    public HotelImageOutputDto HotelImage { get; set; } = default!;
}

using HotelBookingPlatform.Application.DTOs.Location;
using HotelBookingPlatform.Application.DTOs.Room;

namespace HotelBookingPlatform.Application.DTOs.Hotel;

/// <summary>
/// This class is a data transfer object (DTO) for the <see cref="Domain.Models.Hotel"/> entity. Mainly for Hotel Page.
/// </summary>
public class HotelWithRoomsAndImagesOutputDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public short StarRate { get; set; }
    public string? Description { get; set; }

    public string CityName { get; set; } = default!;
    public string? Street { get; set; }
    public LocationDto Location { get; set; } = default!;

    public ICollection<HotelImageOutputDto> Images { get; set; } = default!;
    public ICollection<RoomWithImageOutputDto> Rooms { get; set; } = default!;
}

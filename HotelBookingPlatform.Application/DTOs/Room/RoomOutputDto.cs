using HotelBookingPlatform.Domain.Enums;

namespace HotelBookingPlatform.Application.DTOs.Room;

/// <summary>
/// This class is a data transfer object (DTO) for the <see cref="Domain.Models.Room"/> entity. Mainly for Admin Page.
/// </summary>
public class RoomOutputDto
{
    public Guid Id { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModified { get; set; }
    public int RoomNumber { get; set; }
    public RoomType RoomType { get; set; }
    public int AdultsCapacity { get; set; }
    public int ChildrenCapacity { get; set; }
    public string HotelName { get; set; } = default!;
    public decimal Price { get; set; }
}

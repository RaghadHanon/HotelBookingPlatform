namespace HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Domain.Models;

public class CurrentGuestDto
{
    public string UserId { get; set; } = default!;
    public Guest Guest { get; set; } = default!;
}

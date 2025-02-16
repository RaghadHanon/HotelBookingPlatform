namespace HotelBookingPlatform.Application.DTOs.Guest;

public class GuestOutputDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateOnly DateOfBirth { get; set; } = default!;
    public string Address { get; set; } = default!;
    public int NumberOfBookings { get; set; }
}

namespace HotelBookingPlatform.Application.DTOs.Booking;

public class BookingOutputDto
{
    public Guid ConfirmationId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
    public string GuestFullName { get; set; } = default!;
    public Guid GuestId { get; set; }
    public string HotelName { get; set; } = default!;
    public Guid HotelId { get; set; }
    public decimal Price { get; set; }
    public ICollection<int> RoomNumbers { get; set; } = new List<int>();

}
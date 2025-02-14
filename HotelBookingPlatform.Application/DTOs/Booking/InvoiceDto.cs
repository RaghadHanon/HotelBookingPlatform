using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.DTOs.Room;

namespace HotelBookingPlatform.Application.DTOs.Booking;

/// <summary>
/// This class is a data transfer object (DTO) for the <see cref="Domain.Models.Booking"/> entity.
/// </summary>
public class InvoiceDto
{
    public Guid ConfirmationId { get; set; } // Booking Id
    public decimal TotalPrice { get; set; }
    public decimal TotalPriceAfterDiscount { get; set; }
    public List<RoomWithinInvoiceDto> Rooms { get; set; } = default!;
    public HotelWithinInvoiceDto Hotel { get; set; } = default!;
    public Guid GuestId { get; set; }
    public string GuestFullName { get; set; } = default!;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int NumberOfAdults { get; set; }
    public int NumberOfChildren { get; set; }
}

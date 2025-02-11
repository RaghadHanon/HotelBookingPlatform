using HotelBookingPlatform.Application.DTOs.Booking;

namespace HotelBookingPlatform.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingOutputDto> CreateBookingAsync(CreateBookingDto request);
        Task<bool> DeleteBookingAsync(Guid bookingId);
        Task<BookingOutputDto?> GetBookingAsync(Guid bookingId);
        Task<InvoiceDto?> GetInvoiceAsync(Guid bookingId);
        Task<byte[]> GetInvoicePdfByBookingIdAsync(Guid bookingId);
    }
}
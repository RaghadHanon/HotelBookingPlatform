using HotelBookingPlatform.Application.DTOs.Booking;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Strategies;

public interface IPdfGenerationStrategy
{
    byte[] GeneratePdf(InvoiceDto invoice);
}

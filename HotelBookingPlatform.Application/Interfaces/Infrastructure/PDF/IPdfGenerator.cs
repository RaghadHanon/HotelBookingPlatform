using HotelBookingPlatform.Application.DTOs.Booking;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;

public interface IPdfGenerator
{
    byte[] GeneratePdf(InvoiceDto invoice);
}
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Enums;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;

public interface IPdfGenerator
{
    byte[] GeneratePdf(InvoiceDto invoice, PdfType pdfType);
}
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IInvoiceService
{
    InvoiceDto GenerateInvoice(Booking booking);
    byte[] GenerateInvoicePdf(InvoiceDto invoice);
}

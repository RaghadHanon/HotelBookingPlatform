using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;
public class InvoiceService : IInvoiceService
{
    private readonly IMapper _mapper;
    private readonly IPdfGenerator _pdfService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IMapper mapper, IPdfGenerator pdfService, ILogger<InvoiceService> logger)
    {
        _mapper = mapper;
        _pdfService = pdfService;
        _logger = logger;
    }

    public InvoiceDto GenerateInvoice(Booking booking)
    {
        _logger.LogInformation(ApplicationLogMessages.ConvertingBookingToInvoiceStart, booking.Id);
        InvoiceDto invoice = _mapper.Map<InvoiceDto>(booking);
        invoice.Rooms = ConvertRoomsToInvoice(booking);
        invoice.TotalPrice = invoice.Rooms.Sum(r => r.TotalRoomPrice);
        invoice.TotalPriceAfterDiscount = booking.Price;
        _logger.LogInformation(ApplicationLogMessages.ConvertingBookingToInvoiceSuccess, invoice);

        return invoice;
    }

    public byte[] GenerateInvoicePdf(InvoiceDto invoice)
    {
        _logger.LogDebug(ApplicationLogMessages.PdfGenerationStart, invoice.ConfirmationId);
        byte[] pdfBytes = _pdfService.GeneratePdf(invoice, PdfType.InvoicePdf);
        _logger.LogInformation(ApplicationLogMessages.PdfGenerationSuccess, invoice.ConfirmationId);

        return pdfBytes;
    }

    private List<RoomWithinInvoiceDto> ConvertRoomsToInvoice(Booking booking)
    {
        return booking.BookingRooms.Select(bookingRoom => ConvertRoomToInvoice(bookingRoom, booking.CheckInDate, booking.CheckOutDate)).ToList();
    }

    private RoomWithinInvoiceDto ConvertRoomToInvoice(BookingRoom bookingRoom, DateOnly checkInDate, DateOnly checkOutDate)
    {
        _logger.LogInformation(ApplicationLogMessages.StartingRoomWithinInvoiceConversion, bookingRoom.Room.Id);
        _logger.LogDebug(ApplicationLogMessages.RoomPricePerNightRetrieval, bookingRoom.Room.Id);
        decimal pricePerNight = bookingRoom.FinalPrice;
        int numberOfNights = checkOutDate.DayNumber - checkInDate.DayNumber;
        _logger.LogDebug(ApplicationLogMessages.CalculatedNumberOfNights, numberOfNights, bookingRoom.Room.Id);
        RoomWithinInvoiceDto roomInvoice = _mapper.Map<RoomWithinInvoiceDto>(bookingRoom);
        roomInvoice.PricePerNightAfterDiscount = pricePerNight;
        roomInvoice.NumberOfNights = numberOfNights;
        roomInvoice.TotalRoomPrice = bookingRoom.Room.Price * numberOfNights;
        roomInvoice.TotalRoomPriceAfterDiscount = pricePerNight * numberOfNights;
        _logger.LogInformation(ApplicationLogMessages.RoomWithinInvoiceCreationSuccess, bookingRoom.Room.Id, roomInvoice);

        return roomInvoice;
    }
}

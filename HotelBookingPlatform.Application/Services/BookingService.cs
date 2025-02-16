using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Bookings;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class BookingService : IBookingService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly IGuestService _guestService;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;
    private readonly IInvoiceService _invoiceService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BookingServiceValidator _validator;
    private readonly IEntityValidatorService _entityValidatorService;

    public BookingService(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        IMapper mapper,
        IGuestService guestService,
        IEmailService emailService,
        IInvoiceService invoiceService,
        ILogger<BookingService> logger,
        BookingServiceValidator validator,
        IEntityValidatorService entityValidatorService,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _guestService = guestService;
        _emailService = emailService;
        _invoiceService = invoiceService;
        _logger = logger;
        _validator = validator;
        _entityValidatorService = entityValidatorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingOutputDto?> GetBookingAsync(Guid bookingId)
    {
        var booking = await _entityValidatorService.ValidateBookingAsync(bookingId);
        var currentGuestDto = await _guestService.GetGuestFromCurrentUser();
        _validator.EnsureGuestOwnsBooking(booking.GuestId, currentGuestDto);

        return _mapper.Map<BookingOutputDto>(booking);
    }

    public async Task<BookingOutputDto> CreateBookingAsync(CreateBookingDto request)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var booking = await CreateBookingIfValid(request);
            await _bookingRepository.AddBookingAsync(booking);
            await _bookingRepository.SaveChangesAsync();
            var currentGuestDto = await _guestService.GetGuestFromCurrentUser();
            var outputModel = await PostBookingProcess(booking, currentGuestDto.Email);
            await _unitOfWork.CommitTransactionAsync();

            return outputModel;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, ServicesErrorMessages.ErrorDuringBookingCreation);
            throw;
        }
    }

    private async Task<Booking> CreateBookingIfValid(CreateBookingDto request)
    {
        var hotel = await _entityValidatorService.ValidateHotelAsync(request.HotelId);
        var currentGuestDto = await _guestService.GetGuestFromCurrentUser();
        var booking = new Booking(hotel, currentGuestDto.Guest, new List<BookingRoom>())
        {
            NumberOfAdults = request.NumberOfAdults,
            NumberOfChildren = request.NumberOfChildren,
            CheckInDate = DateOnly.FromDateTime(request.CheckInDate),
            CheckOutDate = DateOnly.FromDateTime(request.CheckOutDate),
        };
        var bookingRooms = await FetchBookingRooms(request, booking);
        await _validator.ValidateRooms(request, bookingRooms.Select(br => br.Room).ToList(), hotel);
        booking.Price = CalculateTotalPrice(bookingRooms, booking.CheckInDate, booking.CheckOutDate);
        booking.BookingRooms = bookingRooms;

        return booking;
    }

    private decimal CalculateTotalPrice(List<BookingRoom> bookingRooms, DateOnly checkInDate, DateOnly checkOutDate)
    {
        var numberOfNights = checkOutDate.DayNumber - checkInDate.DayNumber;

        return bookingRooms.Sum(br => br.FinalPrice * numberOfNights);
    }

    private async Task<List<BookingRoom>> FetchBookingRooms(CreateBookingDto request, Booking booking)
    {
        var bookingRooms = new List<BookingRoom>();
        foreach (var roomId in request.RoomIds)
        {
            var room = await _entityValidatorService.ValidateRoomAsync(roomId);
            var activeDiscount = await _roomRepository.GetActiveDiscountForRoom(roomId, request.CheckInDate, request.CheckOutDate);
            var bookingRoom = new BookingRoom(booking, room, activeDiscount);
            bookingRooms.Add(bookingRoom);
        }

        return bookingRooms;
    }

    private async Task<BookingOutputDto> PostBookingProcess(Booking booking, string userEmail)
    {
        var outputModel = _mapper.Map<BookingOutputDto>(booking);
        await SendEmailAsync(booking, userEmail);

        return outputModel;
    }

    private async Task SendEmailAsync(Booking booking, string toEmail)
    {
        _logger.LogInformation(ApplicationLogMessages.SendingEmail, booking.Id, toEmail);
        var invoice = _invoiceService.GenerateInvoice(booking);
        var invoicePdf = _invoiceService.GenerateInvoicePdf(invoice);
        MailRequest mail = new()
        {
            ToEmail = toEmail,
            Subject = "Booking Confirmation",
            Body = "Your booking has been confirmed. Please find the attached invoice for your reference.",
            Attachment = invoicePdf
        };
        await _emailService.SendEmailAsync(mail, EmailType.BookingConfirmationEmail);
        _logger.LogDebug(ApplicationLogMessages.EmailSentSuccess, toEmail);
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid bookingId)
    {
        var booking = await _entityValidatorService.ValidateBookingAsync(bookingId);
        var currentGuestDto = await _guestService.GetGuestFromCurrentUser();
        _validator.EnsureGuestOwnsBooking(booking.GuestId, currentGuestDto);

        return _invoiceService.GenerateInvoice(booking);
    }

    public async Task<byte[]> GetInvoicePdfByBookingIdAsync(Guid bookingId)
    {
        _logger.LogInformation(ApplicationLogMessages.PdfGenerationStart, bookingId);
        var booking = await _entityValidatorService.ValidateBookingAsync(bookingId);
        var currentGuestDto = await _guestService.GetGuestFromCurrentUser();
        _validator.EnsureGuestOwnsBooking(booking.GuestId, currentGuestDto);
        var invoice = _invoiceService.GenerateInvoice(booking);

        return _invoiceService.GenerateInvoicePdf(invoice);
    }

    public async Task<bool> DeleteBookingAsync(Guid bookingId)
    {
        _logger.LogInformation(ApplicationLogMessages.DeletingBooking, bookingId);
        var booking = await _bookingRepository.GetBookingAsync(bookingId);
        if (booking == null)
            return false;

        var currentGuestDto = await _guestService.GetGuestFromCurrentUser();
        _validator.EnsureGuestOwnsBooking(booking.GuestId, currentGuestDto);
        _validator.EnsureBookingCanBeDeleted(booking.CheckInDate);
        await _bookingRepository.DeleteBookingAsync(bookingId);
        await _bookingRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.DeleteBookingSuccess, bookingId);

        return true;
    }
}
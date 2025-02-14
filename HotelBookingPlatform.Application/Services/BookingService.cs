using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Validators.Bookings;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class BookingService : IBookingService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;
    private readonly BookingServiceValidator _validator;
    private readonly IInvoiceService _invoiceService;

    public BookingService(IHotelRepository hotelRepository, IRoomRepository roomRepository, IGuestRepository guestRepository, IBookingRepository bookingRepository, IMapper mapper, ICurrentUser currentUser, IEmailService emailService, IInvoiceService invoiceService, ILogger<BookingService> logger, BookingServiceValidator validator)
    {
        _hotelRepository = hotelRepository;
        _roomRepository = roomRepository;
        _guestRepository = guestRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _emailService = emailService;
        _invoiceService = invoiceService;
        _logger = logger;
        _validator = validator;
    }

    public async Task<BookingOutputDto?> GetBookingAsync(Guid bookingId)
    {
        Booking booking = await _bookingRepository.GetBookingAsync(bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);
        CurrentGuestDto currentGuestDto = await GetGuestFromCurrentUser();
        if (booking.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, nameof(Booking), booking.Id);
        }

        return _mapper.Map<BookingOutputDto>(booking);
    }

    private async Task<CurrentGuestDto> GetGuestFromCurrentUser()
    {
        _logger.LogDebug(ApplicationLogMessages.GuestIdRetrieval);
        string userId = _currentUser.Id;
        _logger.LogDebug(ApplicationLogMessages.GuestIdMatchCheck);
        Guest guest = await _guestRepository.GetGuestByUserIdAsync(userId)
            ?? throw new NotFoundException(nameof(Guest), userId);

        return new CurrentGuestDto { Guest = guest, UserId = userId };
    }

    public async Task<BookingOutputDto> CreateBookingAsync(CreateBookingDto request)
    {
        await _bookingRepository.BeginTransactionAsync();
        try
        {
            Booking booking = await ValidateAndCreateBooking(request);
            await _bookingRepository.AddBookingAsync(booking);
            await _bookingRepository.SaveChangesAsync();
            BookingOutputDto outputModel = await PostBookingProcess(booking, _currentUser.Email);
            await _bookingRepository.CommitTransactionAsync();

            return outputModel;
        }
        catch (Exception ex)
        {
            await _bookingRepository.RollbackTransactionAsync();
            _logger.LogError(ex, ServicesErrorMessages.ErrorDuringBookingCreation);
            throw;
        }
    }

    private async Task<Booking> ValidateAndCreateBooking(CreateBookingDto request)
    {
        Hotel hotel = await _hotelRepository.GetHotelAsync(request.HotelId)
            ?? throw new NotFoundException(nameof(Hotel), request.HotelId);
        CurrentGuestDto currentGuestDto = await GetGuestFromCurrentUser();
        Booking booking = new Booking(hotel, currentGuestDto.Guest, new List<BookingRoom>())
        {
            NumberOfAdults = request.NumberOfAdults,
            NumberOfChildren = request.NumberOfChildren,
            CheckInDate = DateOnly.FromDateTime(request.CheckInDate),
            CheckOutDate = DateOnly.FromDateTime(request.CheckOutDate),
        };
        List<BookingRoom> bookingRooms = await FetchBookingRooms(request, booking);
        await _validator.ValidateRooms(request, bookingRooms.Select(br => br.Room).ToList(), hotel);
        booking.Price = CalculateTotalPrice(bookingRooms, booking.CheckInDate, booking.CheckOutDate);
        booking.BookingRooms = bookingRooms;

        return booking;
    }

    private async Task<List<BookingRoom>> FetchBookingRooms(CreateBookingDto request, Booking booking)
    {
        List<BookingRoom> bookingRooms = new List<BookingRoom>();
        foreach (Guid roomId in request.RoomIds)
        {
            Room room = await _roomRepository.GetRoomAsync(roomId)
                ?? throw new ServerErrorException(ServicesErrorMessages.RoomsCannotBeNull);
            Discount? activeDiscount = await _roomRepository.GetActiveDiscountForRoom(roomId, request.CheckInDate, request.CheckOutDate);
            BookingRoom bookingRoom = new BookingRoom(booking, room, activeDiscount);
            bookingRooms.Add(bookingRoom);
        }

        return bookingRooms;
    }

    private decimal CalculateTotalPrice(List<BookingRoom> bookingRooms, DateOnly checkInDate, DateOnly checkOutDate)
    {
        decimal totalPrice = 0m;
        int numberOfNights = checkOutDate.DayNumber - checkInDate.DayNumber;
        foreach (BookingRoom bookingRoom in bookingRooms)
        {
            decimal pricePerNight = bookingRoom.FinalPrice;
            decimal roomTotalPrice = pricePerNight * numberOfNights;
            totalPrice += roomTotalPrice;
        }

        return totalPrice;
    }

    private async Task<BookingOutputDto> PostBookingProcess(Booking booking, string userEmail)
    {
        BookingOutputDto outputModel = _mapper.Map<BookingOutputDto>(booking);
        await SendEmailAsync(booking, userEmail);

        return outputModel;
    }

    private async Task SendEmailAsync(Booking booking, string toEmail)
    {
        _logger.LogInformation(ApplicationLogMessages.SendingEmail, booking.Id, toEmail);
        InvoiceDto invoice = _invoiceService.GenerateInvoice(booking);
        byte[] invoicePdf = _invoiceService.GenerateInvoicePdf(invoice);
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
        Booking booking = await _bookingRepository.GetBookingAsync(bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);
        CurrentGuestDto currentGuestDto = await GetGuestFromCurrentUser();
        if (booking.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, nameof(Booking), booking.Id);
        }

        var invoice = _invoiceService.GenerateInvoice(booking);

        return invoice;
    }

    public async Task<byte[]> GetInvoicePdfByBookingIdAsync(Guid bookingId)
    {
        _logger.LogInformation(ApplicationLogMessages.PdfGenerationStart, bookingId);
        Booking booking = await _bookingRepository.GetBookingAsync(bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);
        CurrentGuestDto currentGuestDto = await GetGuestFromCurrentUser();
        if (booking.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, booking.GuestId);
        }

        var invoice = _invoiceService.GenerateInvoice(booking);
        return _invoiceService.GenerateInvoicePdf(invoice);
    }

    public async Task<bool> DeleteBookingAsync(Guid bookingId)
    {
        _logger.LogInformation(ApplicationLogMessages.DeletingBooking, bookingId);
        Booking? booking = await _bookingRepository.GetBookingAsync(bookingId);
        if (booking == null)
            return false;

        await CanGuestDeleteTheBooking(booking);
        await _bookingRepository.DeleteBookingAsync(bookingId);
        await _bookingRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.DeleteBookingSuccess, bookingId);

        return true;
    }

    private async Task CanGuestDeleteTheBooking(Booking booking)
    {
        _logger.LogDebug(ApplicationLogMessages.GuestIdRetrieval);
        CurrentGuestDto currentGuestDto = await GetGuestFromCurrentUser();
        _logger.LogDebug(ApplicationLogMessages.GuestIdMatchCheck);
        if (currentGuestDto.Guest.Id != booking.GuestId)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, booking.GuestId);
        }

        if (booking.CheckInDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BadRequestException(ServicesErrorMessages.CannotDeleteStartedBooking);
        }
    }
}
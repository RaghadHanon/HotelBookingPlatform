using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
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
    private readonly IPdfGenerator _pdfService;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;
    private readonly BookingServiceValidator _validator;

    public BookingService(IHotelRepository hotelRepository, IRoomRepository roomRepository, IGuestRepository guestRepository, IBookingRepository bookingRepository, IMapper mapper, ICurrentUser currentUser, IPdfGenerator pdfService, IEmailService emailService, ILogger<BookingService> logger, BookingServiceValidator validator)
    {
        _hotelRepository = hotelRepository;
        _roomRepository = roomRepository;
        _guestRepository = guestRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _pdfService = pdfService;
        _emailService = emailService;
        _logger = logger;
        _validator = validator;
    }

    public async Task<BookingOutputDto?> GetBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetBookingAsync(bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);
        var currentGuestDto = await GetGuestFromCurrentUser();
        if (booking.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, nameof(Booking), booking.Id);
        }

        return _mapper.Map<BookingOutputDto>(booking);
    }

    private async Task<CurrentGuestDto> GetGuestFromCurrentUser()
    {
        _logger.LogDebug(ApplicationLogMessages.GuestIdRetrieval);
        var userId = _currentUser.Id;
        _logger.LogDebug(ApplicationLogMessages.GuestIdMatchCheck);
        var guest = await _guestRepository.GetGuestByUserIdAsync(userId)
            ?? throw new NotFoundException(nameof(Guest), userId);

        return new CurrentGuestDto { Guest = guest, UserId = userId };
    }

    public async Task<BookingOutputDto> CreateBookingAsync(CreateBookingDto request)
    {
        await _bookingRepository.BeginTransactionAsync();
        try
        {
            var booking = await ValidateAndCreateBooking(request);
            await _bookingRepository.AddBookingAsync(booking);
            await _bookingRepository.SaveChangesAsync();
            var outputModel = await PostBookingProcess(booking, _currentUser.Email);
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

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetBookingAsync(bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);
        var currentGuestDto = await GetGuestFromCurrentUser();
        if (booking.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, nameof(Booking), booking.Id);
        }

        return ConvertBookingToInvoice(booking);
    }

    private InvoiceDto ConvertBookingToInvoice(Booking booking)
    {
        _logger.LogInformation(ApplicationLogMessages.ConvertingBookingToInvoiceStart, booking.Id);
        var invoice = _mapper.Map<InvoiceDto>(booking);
        _logger.LogDebug(ApplicationLogMessages.ConvertingBookingToInvoiceSuccess, invoice);
        invoice.Rooms = ConvertRoomsToRoomsWithinInvoice(booking);
        invoice.TotalPrice = invoice.Rooms.Sum(r => r.TotalRoomPrice);
        invoice.TotalPriceAfterDiscount = booking.Price;
        _logger.LogInformation(ApplicationLogMessages.ConvertingBookingToInvoiceSuccess, invoice);

        return invoice;
    }

    private List<RoomWithinInvoiceDto> ConvertRoomsToRoomsWithinInvoice(Booking booking)
    {
        var roomsWithinInvoice = new List<RoomWithinInvoiceDto>();
        foreach (var bookingRoom in booking.BookingRooms)
        {
            var roomInvoice = ConvertRoomToRoomWithinInvoice(
                bookingRoom,
                booking.CheckInDate,
                booking.CheckOutDate
            );
            roomsWithinInvoice.Add(roomInvoice);
        }

        return roomsWithinInvoice;
    }

    private RoomWithinInvoiceDto ConvertRoomToRoomWithinInvoice(BookingRoom bookingRoom, DateOnly checkInDate, DateOnly checkOutDate)
    {
        _logger.LogInformation(ApplicationLogMessages.StartingRoomWithinInvoiceConversion, bookingRoom.Room.Id);
        _logger.LogDebug(ApplicationLogMessages.RoomPricePerNightRetrieval, bookingRoom.Room.Id);
        var pricePerNight = bookingRoom.FinalPrice;
        var numberOfNights = checkOutDate.DayNumber - checkInDate.DayNumber;
        _logger.LogDebug(ApplicationLogMessages.CalculatedNumberOfNights, numberOfNights, bookingRoom.Room.Id);
        var roomInInvoice = _mapper.Map<RoomWithinInvoiceDto>(bookingRoom);
        roomInInvoice.PricePerNightAfterDiscount = pricePerNight;
        roomInInvoice.NumberOfNights = numberOfNights;
        roomInInvoice.TotalRoomPrice = bookingRoom.Room.Price * numberOfNights;
        roomInInvoice.TotalRoomPriceAfterDiscount = pricePerNight * numberOfNights;
        _logger.LogInformation(ApplicationLogMessages.RoomWithinInvoiceCreationSuccess, bookingRoom.Room.Id, roomInInvoice);

        return roomInInvoice;
    }

    private async Task<Booking> ValidateAndCreateBooking(CreateBookingDto request)
    {
        var hotel = await _hotelRepository.GetHotelAsync(request.HotelId)
            ?? throw new NotFoundException(nameof(Hotel), request.HotelId);
        var currentGuestDto = await GetGuestFromCurrentUser();
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

    private async Task<List<BookingRoom>> FetchBookingRooms(CreateBookingDto request, Booking booking)
    {
        var bookingRooms = new List<BookingRoom>();
        foreach (var roomId in request.RoomIds)
        {
            var room = await _roomRepository.GetRoomAsync(roomId)
                ?? throw new ServerErrorException(ServicesErrorMessages.RoomsCannotBeNull);
            Discount? activeDiscount = await _roomRepository.GetActiveDiscountForRoom(roomId, request.CheckInDate, request.CheckOutDate);
            var bookingRoom = new BookingRoom(booking, room, activeDiscount);
            bookingRooms.Add(bookingRoom);
        }

        return bookingRooms;
    }

    private decimal CalculateTotalPrice(List<BookingRoom> bookingRooms, DateOnly checkInDate, DateOnly checkOutDate)
    {
        var totalPrice = 0m;
        var numberOfNights = checkOutDate.DayNumber - checkInDate.DayNumber;
        foreach (var bookingRoom in bookingRooms)
        {
            var pricePerNight = bookingRoom.FinalPrice;
            var roomTotalPrice = pricePerNight * numberOfNights;
            totalPrice += roomTotalPrice;
        }

        return totalPrice;
    }

    private async Task<BookingOutputDto> PostBookingProcess(Booking booking, string userEmail)
    {
        var outputModel = _mapper.Map<BookingOutputDto>(booking);
        await SendEmailAsync(booking, userEmail);

        return outputModel;
    }

    private async Task SendEmailAsync(Booking booking, string toEmail)
    {
        _logger.LogDebug(ApplicationLogMessages.PdfGenerationStart, booking.Id);
        var invoice = ConvertBookingToInvoice(booking);
        _logger.LogInformation(ApplicationLogMessages.SendingEmail, invoice, toEmail);
        var invoicePdf = GetInvoicePdf(invoice);
        MailRequest mail = new()
        {
            ToEmail = toEmail,
            Subject = "Booking Confirmation",
            Body = "Your booking has been confirmed. Please find the attached invoice for your reference. If you have any questions, feel free to contact us. Thank you for choosing our service!",
            Attachment = invoicePdf
        };
        await _emailService.SendEmailAsync(mail);
        _logger.LogDebug(ApplicationLogMessages.EmailSentSuccess, toEmail);
    }

    private byte[] GetInvoicePdf(InvoiceDto invoice)
    {
        _logger.LogDebug(ApplicationLogMessages.PdfGenerationStart, invoice.ConfirmationId);
        var pdfBytes = _pdfService.GeneratePdf(invoice);
        _logger.LogInformation(ApplicationLogMessages.PdfGenerationSuccess, invoice.ConfirmationId);

        return pdfBytes;
    }

    public async Task<byte[]> GetInvoicePdfByBookingIdAsync(Guid bookingId)
    {
        _logger.LogInformation(ApplicationLogMessages.PdfGenerationStart, bookingId);
        var booking = await _bookingRepository.GetBookingAsync(bookingId)
            ?? throw new NotFoundException(nameof(Booking), bookingId);
        var currentGuestDto = await GetGuestFromCurrentUser();
        if (booking.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, booking.GuestId);
        }

        var invoice = ConvertBookingToInvoice(booking);

        return GetInvoicePdf(invoice);
    }

    public async Task<bool> DeleteBookingAsync(Guid bookingId)
    {
        _logger.LogInformation(ApplicationLogMessages.DeletingBooking, bookingId);
        var booking = await _bookingRepository.GetBookingAsync(bookingId);
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
        var currentGuestDto = await GetGuestFromCurrentUser();
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
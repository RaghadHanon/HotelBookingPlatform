using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Email;
using HotelBookingPlatform.Application.Enums;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Application.Validators.Bookings;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests.BookingServiceTests;

public class BookingServiceTests
{
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IInvoiceService> _invoiceServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly BookingServiceValidator _validator;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<BookingService>> _loggerMock;
    private readonly IFixture _fixture;
    private readonly BookingService sut;

    public BookingServiceTests()
    {
        _fixture = FixtureFactory.CreateFixture();
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _invoiceServiceMock = new Mock<IInvoiceService>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _loggerMock = new Mock<ILogger<BookingService>>();
        _validator = new BookingServiceValidator(_roomRepositoryMock.Object, new Mock<ILogger<BookingServiceValidator>>().Object);

        sut = new BookingService(_hotelRepositoryMock.Object,
                                 _roomRepositoryMock.Object,
                                 _guestRepositoryMock.Object,
                                 _bookingRepositoryMock.Object,
                                 _mapper,
                                 _currentUserMock.Object,
                                 _emailServiceMock.Object,
                                 _invoiceServiceMock.Object,
                                 _loggerMock.Object,
                                 _validator);
    }

    [Fact]
    public async Task GetBookingAsync_ShouldReturnBookingOutputModel_WhenBookingExists()
    {
        // Arrange
        string userId = "jgjy-kgjg-lkyf";
        Guest guest = _fixture.Build<Guest>()
            .Without(g => g.Bookings)
            .Create();
        Booking booking = _fixture.Build<Booking>()
            .Without(b => b.BookingRooms)
            .Without(b => b.Hotel)
            .With(b => b.Guest, guest)
            .With(b => b.GuestId, guest.Id)
            .Create();
        _guestRepositoryMock
            .Setup(x => x.GetGuestByUserIdAsync(userId))
            .ReturnsAsync(guest);
        _currentUserMock
            .Setup(x => x.Id)
            .Returns(userId);
        _bookingRepositoryMock
            .Setup(x => x.GetBookingAsync(booking.Id))
            .ReturnsAsync(booking);

        // Act
        BookingOutputDto? result = await sut.GetBookingAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BookingOutputDto>(result);
        _bookingRepositoryMock.Verify(
            x => x.GetBookingAsync(booking.Id),
            Times.Once
        );
    }

    [Fact]
    public async Task GetBookingAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(bookingId)).ReturnsAsync((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetBookingAsync(bookingId));
        _bookingRepositoryMock.Verify(x => x.GetBookingAsync(bookingId), Times.Once);
    }

    [Fact]
    public async Task GetBookingAsync_ShouldThrowUnauthorizedException_WhenCurrentUserIsNotOwnerOfBooking()
    {
        // Arrange
        Guest unauthorizedGuest = _fixture.Build<Guest>()
            .Without(g => g.Bookings)
            .Create();
        Booking booking = _fixture.Build<Booking>()
            .Without(b => b.BookingRooms)
            .Without(b => b.Hotel)
            .Without(b => b.Guest)
            .Without(b => b.GuestId)// different GuestId than the current user id 
            .Create();
        string unauthorizedUserId = "kjhkhf-kjenkjg-kjngk";

        _currentUserMock.SetupGet(x => x.Id).Returns(unauthorizedUserId);
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(unauthorizedUserId)).ReturnsAsync(unauthorizedGuest);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.GetBookingAsync(booking.Id));
    }

    [Fact]
    public async Task GetInvoicePdf_ShouldReturnPdfBytes_WhenBookingExistsAndUserIsAuthorized()
    {
        // Arrange
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        Booking booking = _fixture.Build<Booking>()
             .Without(b => b.BookingRooms)
             .Without(b => b.Hotel)
             .With(b => b.Guest, guest)
             .With(b => b.GuestId, guest.Id)
             .Create();
        InvoiceDto invoice = _fixture.Create<InvoiceDto>();
        string userId = "jgjy-kgjg-lkyf";
        byte[] pdfBytes = new byte[100];
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(guest);
        _currentUserMock.Setup(x => x.Id).Returns(userId);
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _invoiceServiceMock.Setup(x => x.GenerateInvoicePdf(It.IsAny<InvoiceDto>())).Returns(pdfBytes);

        // Act
        byte[] result = await sut.GetInvoicePdfByBookingIdAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<byte[]>(result);
        _bookingRepositoryMock.Verify(x => x.GetBookingAsync(booking.Id), Times.Once);
        _invoiceServiceMock.Verify(x => x.GenerateInvoicePdf(It.IsAny<InvoiceDto>()), Times.Once);
    }

    [Fact]
    public async Task GetInvoicePdf_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(bookingId)).ReturnsAsync((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetInvoicePdfByBookingIdAsync(bookingId));
    }

    [Fact]
    public async Task GetInvoicePdf_ShouldThrowUnauthorizedException_WhenCurrentUserIsNotGuestOfBooking()
    {
        // Arrange
        Guest unauthorizedGuest = _fixture.Build<Guest>()
            .Without(g => g.Bookings)
            .Create();
        Booking booking = _fixture.Build<Booking>()
            .Without(b => b.BookingRooms)
            .Without(b => b.Hotel)
            .Without(b => b.Guest)
            .Without(b => b.GuestId)// different GuestId than the current user id 
            .Create();
        string unauthorizedUserId = "kjhkhf-kjenkjg-kjngk";

        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(unauthorizedUserId)).ReturnsAsync(unauthorizedGuest);
        _currentUserMock.SetupGet(x => x.Id).Returns(unauthorizedUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.GetInvoicePdfByBookingIdAsync(booking.Id));
    }

    [Fact]
    public async Task DeleteBookingAsync_ShouldReturnTrue_WhenUserIsAuthorizedAndBookingIsSuccessfullyDeleted()
    {
        // Arrange
        Guid guestId = Guid.NewGuid();
        List<Booking> bookings = _fixture.Build<Booking>()
                       .With(b => b.GuestId, guestId)
                       .With(b => b.CheckInDate, DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
                       .With(b => b.CheckOutDate, DateOnly.FromDateTime(DateTime.Today.AddDays(3)))
                       .Without(b => b.BookingRooms)
                       .Without(b => b.Hotel)
                       .CreateMany(1)
                       .ToList();
        Guest guest = _fixture.Build<Guest>()
                   .With(g => g.Id, guestId)
                   .With(g => g.Bookings, bookings)
                   .Create();
        string userId = "uyey-dksjj-uryt";
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(bookings[0].Id)).ReturnsAsync(bookings[0]);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(guest);
        _currentUserMock.SetupGet(x => x.Id).Returns(userId);

        // Act
        bool result = await sut.DeleteBookingAsync(bookings[0].Id);

        // Assert
        Assert.True(result);
        _bookingRepositoryMock.Verify(x => x.DeleteBookingAsync(bookings[0].Id), Times.Once);
        _bookingRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteBookingAsync_ShouldThrowUnauthorizedException_WhenCurrentUserIsNotGuestOfBooking()
    {
        // Arrange
        Guest unauthorizedGuest = _fixture.Build<Guest>()
            .Without(g => g.Bookings)
            .Create();
        Booking booking = _fixture.Build<Booking>()
            .Without(b => b.BookingRooms)
            .Without(b => b.Hotel)
            .Without(b => b.Guest)
            .Without(b => b.GuestId)// different GuestId than the current user id 
            .Create();
        string unauthorizedUserId = "kjhkhf-kjenkjg-kjngk";

        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _currentUserMock.SetupGet(x => x.Id).Returns(unauthorizedUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(unauthorizedUserId)).ReturnsAsync(unauthorizedGuest);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.DeleteBookingAsync(booking.Id));
    }

    [Fact]
    public async Task DeleteBookingAsync_ShouldReturnFalse_WhenBookingDoesNotExist()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(bookingId)).ReturnsAsync((Booking?)null);

        // Act & Assert
        bool result = await sut.DeleteBookingAsync(bookingId);
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteBookingAsync_ShouldThrowBadRequestException_WhenTryingToDeleteBookingAfterCheckIn()
    {
        // Arrange
        string userId = "uyey-dksjj-uryt";
        Guest guest = _fixture.Build<Guest>()
                    .Without(g => g.Bookings)
                    .Create();
        Booking booking = _fixture.Build<Booking>()
                     .Without(b => b.BookingRooms)
                     .Without(b => b.Hotel)
                     .With(b => b.GuestId, guest.Id)
                     .With(b => b.Guest, guest)
                     .With(b => b.CheckInDate, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))) // Booking has already started
                     .With(b => b.CheckOutDate, DateOnly.FromDateTime(DateTime.Today.AddDays(2)))
                     .Create();
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(guest);
        _currentUserMock.SetupGet(x => x.Id).Returns(userId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.DeleteBookingAsync(booking.Id));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldPersistBookingAndReturnBookingConfirmation_WhenBookingRoomsRequestIsValid()
    {
        // Arrange
        Hotel hotel = _fixture.Build<Hotel>()
                    .Without(r => r.Images)
                    .Without(r => r.City)
                    .Without(r => r.Location)
                    .Without(r => r.Rooms)
                    .Without(r => r.Reviews)
                    .Without(r => r.Amenities)
                    .Create();
        List<Room> room = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.BookingRooms)
                   .With(r => r.Hotel, hotel)
                   .With(r => r.HotelId, hotel.Id)
                   .With(r => r.AdultsCapacity, 2)
                   .With(r => r.ChildrenCapacity, 2)
                   .Without(r => r.Amenities)
                   .CreateMany(2)
                   .ToList();
        CreateBookingDto request = _fixture.Build<CreateBookingDto>()
                   .With(x => x.RoomIds, [room[0].Id, room[1].Id])
                   .With(x => x.HotelId, hotel.Id)
                   .With(x => x.CheckInDate, DateTime.Today.AddDays(1))
                   .With(x => x.CheckOutDate, DateTime.Today.AddDays(6))
                   .With(x => x.NumberOfChildren, 2)
                   .With(x => x.NumberOfAdults, 3)
                   .Create();
        Guest guest = _fixture.Build<Guest>()
                   .Without(g => g.Bookings)
                   .Create();

        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(guest);
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(request.HotelId))
            .ReturnsAsync(hotel);
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room[0].Id))
            .ReturnsAsync(room[0]);
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room[1].Id))
            .ReturnsAsync(room[1]);
        _roomRepositoryMock.Setup(x => x.IsAvailableAsync(room[0].Id,
                DateOnly.FromDateTime(request.CheckInDate), DateOnly.FromDateTime(request.CheckOutDate)))
            .ReturnsAsync(true);
        _roomRepositoryMock.Setup(x => x.IsAvailableAsync(room[1].Id,
                DateOnly.FromDateTime(request.CheckInDate), DateOnly.FromDateTime(request.CheckOutDate)))
            .ReturnsAsync(true);
        _bookingRepositoryMock.Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        _bookingRepositoryMock.Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act
        BookingOutputDto result = await sut.CreateBookingAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<BookingOutputDto>(result);
        _bookingRepositoryMock.Verify(x => x.AddBookingAsync(It.IsAny<Booking>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<MailRequest>(), EmailType.BookingConfirmationEmail), Times.Once);
        _invoiceServiceMock.Verify(x => x.GenerateInvoicePdf(It.IsAny<InvoiceDto>()), Times.Once);
        _bookingRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowNotFoundException_WhenHotelDoesNotExist()
    {
        // Arrange
        CreateBookingDto request = _fixture.Create<CreateBookingDto>();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(request.HotelId)).ReturnsAsync((Hotel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateBookingAsync(request));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        CreateBookingDto request = _fixture.Create<CreateBookingDto>();
        Hotel hotel = _fixture.Build<Hotel>()
                   .Without(r => r.Images)
                   .Without(r => r.City)
                   .Without(r => r.Location)
                   .Without(r => r.Rooms)
                   .Without(r => r.Reviews)
                   .Without(r => r.Amenities)
                   .Create();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(request.HotelId)).ReturnsAsync(hotel);
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(It.IsAny<Guid>())).ReturnsAsync((Room?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateBookingAsync(request));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowUnavailableRoomException_WhenRoomIsNotAvailable()
    {
        // Arrange
        Hotel hotel = _fixture.Build<Hotel>()
                    .Without(r => r.Images)
                    .Without(r => r.City)
                    .Without(r => r.Location)
                    .Without(r => r.Rooms)
                    .Without(r => r.Reviews)
                    .Without(r => r.Amenities)
                    .Create();
        Room room = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.BookingRooms)
                   .With(r => r.Hotel, hotel)
                   .With(r => r.HotelId, hotel.Id)
                   .Without(r => r.Amenities)
                   .Create();
        CreateBookingDto request = _fixture.Build<CreateBookingDto>()
                   .With(x => x.RoomIds, [room.Id])
                   .With(x => x.HotelId, hotel.Id)
                   .Create();
        Guest guest = _fixture.Build<Guest>()
                   .Without(g => g.Bookings)
                   .Create();
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync(guest);
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(request.HotelId)).ReturnsAsync(hotel);
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);
        _roomRepositoryMock.Setup(x => x.IsAvailableAsync(room.Id, DateOnly.FromDateTime(request.CheckInDate), DateOnly.FromDateTime(request.CheckOutDate))).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnavailableRoomException>(() => sut.CreateBookingAsync(request));
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    [InlineData(3, 3)]
    public async Task CreateBookingAsync_ShouldThrowInvalidNumberOfGuestsException_WhenRoomCapacityIsExceeded(int numberOfAdults, int numberOfChildren)
    {
        // Arrange
        Hotel hotel = _fixture.Build<Hotel>()
                    .Without(r => r.Images)
                    .Without(r => r.City)
                    .Without(r => r.Location)
                    .Without(r => r.Rooms)
                    .Without(r => r.Reviews)
                    .Without(r => r.Amenities)
                    .Create();
        Room room = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.BookingRooms)
                   .With(r => r.Hotel, hotel)
                   .With(r => r.HotelId, hotel.Id)
                   .With(r => r.ChildrenCapacity, 2)
                   .With(r => r.AdultsCapacity, 2)
                   .Without(r => r.Amenities)
                   .Create();
        CreateBookingDto request = _fixture.Build<CreateBookingDto>()
                   .With(x => x.RoomIds, [room.Id])
                   .With(x => x.HotelId, hotel.Id)
                   .With(x => x.NumberOfAdults, numberOfAdults)
                   .With(x => x.NumberOfChildren, numberOfChildren)
                   .Create();
        Guest guest = _fixture.Build<Guest>()
                   .Without(g => g.Bookings)
                   .Create();
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync(guest);
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(request.HotelId)).ReturnsAsync(hotel);
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);
        _roomRepositoryMock.Setup(x => x.IsAvailableAsync(room.Id, DateOnly.FromDateTime(request.CheckInDate), DateOnly.FromDateTime(request.CheckOutDate))).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidNumberOfGuestsException>(() => sut.CreateBookingAsync(request));
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldRollbackTransaction_WhenInternalErrorOccurs()
    {
        // Arrange
        CreateBookingDto request = _fixture.Create<CreateBookingDto>();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(request.HotelId)).ThrowsAsync(new Exception("Internal Error"));
        _bookingRepositoryMock.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _bookingRepositoryMock.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => sut.CreateBookingAsync(request));
        _bookingRepositoryMock.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateBookingAsync_ShouldThrowNotFoundException_WhenGuestDoesNotExist()
    {
        // Arrange
        CreateBookingDto request = _fixture.Create<CreateBookingDto>();
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Guest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateBookingAsync(request));
    }
}

using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Application.Validators.Bookings;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests.BookingServiceTests;

public class BookingServiceInvoiceCalculationsTests
{
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly IInvoiceService _invoiceService;
    private readonly BookingServiceValidator _validator;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<BookingService>> _loggerMock;
    private readonly IFixture _fixture;
    private readonly BookingService sut;

    public BookingServiceInvoiceCalculationsTests()
    {
        _fixture = FixtureFactory.CreateFixture();
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _loggerMock = new Mock<ILogger<BookingService>>();
        _validator = new BookingServiceValidator(_roomRepositoryMock.Object, new Mock<ILogger<BookingServiceValidator>>().Object);
        _invoiceService = new InvoiceService(_mapper, new Mock<IPdfGenerator>().Object, new Mock<ILogger<InvoiceService>>().Object);

        sut = new BookingService(_hotelRepositoryMock.Object,
                                _roomRepositoryMock.Object,
                                 _guestRepositoryMock.Object,
                                 _bookingRepositoryMock.Object,
                                 _mapper,
                                 _currentUserMock.Object,
                                 _emailServiceMock.Object,
                                 _invoiceService,
                                 _loggerMock.Object,
                                 _validator);
    }
    private Booking CreateBooking()
    {
        Guest guest = _fixture.Build<Guest>()
                   .Without(g => g.Bookings)
                   .Create();

        Room roomWithDiscount = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.Discounts)
                   .Without(r => r.Hotel)
                   .Without(r => r.BookingRooms)
                   .With(r => r.Price, 200)
                   .Without(r => r.Amenities)
                   .Create();
        Discount discount = _fixture.Build<Discount>()
                   .With(d => d.Room, roomWithDiscount)
                   .With(d => d.RoomId, roomWithDiscount.Id)
                   .With(d => d.Percentage, 10)
                   .Create();
        BookingRoom bookingRoomWithDiscount = _fixture.Build<BookingRoom>()
              .With(d => d.Room, roomWithDiscount)
              .With(d => d.RoomId, roomWithDiscount.Id)
              .Without(r => r.Booking)
              .Without(r => r.BookingId)
              .With(r => r.DiscountId, discount.Id)
              .With(r => r.Discount, discount)
              .Without(r => r.FinalPrice)
              .Create();

        Room roomWithoutDiscount = _fixture.Build<Room>()
                  .Without(r => r.Images)
                  .Without(r => r.Discounts)
                  .Without(r => r.Hotel)
                  .Without(r => r.BookingRooms)
                  .With(r => r.Price, 500)
                  .Without(r => r.Amenities)
                  .Create();
        BookingRoom bookingRoomWithoutDiscount = _fixture.Build<BookingRoom>()
              .With(d => d.Room, roomWithoutDiscount)
              .With(d => d.RoomId, roomWithoutDiscount.Id)
              .Without(r => r.Booking)
              .Without(r => r.BookingId)
              .Without(r => r.DiscountId)
              .Without(r => r.Discount)
              .Without(r => r.FinalPrice)
              .Create();

        Booking booking = _fixture.Build<Booking>()
                    .With(b => b.CheckInDate, DateOnly.FromDateTime(DateTime.Today))
                    .With(b => b.CheckOutDate, DateOnly.FromDateTime(DateTime.Today.AddDays(5)))
                    .With(b => b.BookingRooms, new List<BookingRoom> { bookingRoomWithDiscount, bookingRoomWithoutDiscount })
                    .Without(b => b.Hotel)
                    .Without(b => b.HotelId)
                    .With(b => b.Guest, guest)
                    .With(b => b.GuestId, guest.Id)
                    .With(b => b.Price, 3400)
                    .Create();

        return booking;
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldReturnAnInvoice_WhenBookingExists()
    {
        // Arrange
        string userId = "jgjy-kgjg-lkyf";
        Booking booking = CreateBooking();
        InvoiceDto invoice = _fixture.Create<InvoiceDto>();
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(booking.Guest);
        _currentUserMock.Setup(x => x.Id).Returns(userId);
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);

        // Act
        InvoiceDto? result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InvoiceDto>(result);
        _bookingRepositoryMock.Verify(x => x.GetBookingAsync(booking.Id), Times.Once);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectNumberOfNightsInTheReturnedInvoice_WhenBookingRooms()
    {
        // Arrange
        string userId = "jgjy-kgjg-lkyf";
        Booking booking = CreateBooking();
        InvoiceDto invoice = _fixture.Create<InvoiceDto>();
        int expectedNumberOfNights = 5;
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(booking.Guest);
        _currentUserMock.Setup(x => x.Id).Returns(userId);

        // Act
        InvoiceDto? result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        HotelBookingPlatform.Application.DTOs.Room.RoomWithinInvoiceDto firstRoomInvoice = result.Rooms.First();
        Assert.Equal(expectedNumberOfNights, firstRoomInvoice.NumberOfNights);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectTotalPricesInTheReturnedInvoice_WhenBookingRooms()
    {
        // Arrange
        string userId = "jgjy-kgjg-lkyf";
        Booking booking = CreateBooking();
        int numberOfNights = 5;
        decimal expectedTotalRoomsPrice = booking.BookingRooms.Sum(r => r.Room.Price * numberOfNights);
        decimal expectedTotalRoomsPriceAfterDiscounts = booking.BookingRooms.Sum(r => r.FinalPrice * numberOfNights);
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(booking.Guest);
        _currentUserMock.Setup(x => x.Id).Returns(userId);

        // Act & Assert
        InvoiceDto? result = await sut.GetInvoiceAsync(booking.Id);
        Assert.NotNull(result);
        Assert.Equal(expectedTotalRoomsPrice, result.TotalPrice);
        Assert.Equal(expectedTotalRoomsPriceAfterDiscounts, result.TotalPriceAfterDiscount);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectRricesPerRoom_WhenBookingRoomWithoutDiscount()
    {
        // Arrange
        string userId = "jgjy-kgjg-lkyf";
        Booking booking = CreateBooking();
        int numberOfNights = 5;
        decimal expectedPricePerNight = booking.BookingRooms.ToList()[0].Room.Price;
        decimal expectedPricePerNightAfterDiscount = booking.BookingRooms.ToList()[0].FinalPrice;
        decimal expectedTotalRoomPrice = expectedPricePerNight * numberOfNights;
        decimal expectedTotalRoomPriceAfterDiscount = expectedPricePerNightAfterDiscount * numberOfNights;
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(booking.Guest);
        _currentUserMock.Setup(x => x.Id).Returns(userId);

        // Act 
        InvoiceDto? result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        HotelBookingPlatform.Application.DTOs.Room.RoomWithinInvoiceDto roomWithDiscount = result.Rooms[0];
        Assert.NotNull(roomWithDiscount);
        Assert.Equal(expectedPricePerNight, roomWithDiscount.PricePerNight);
        Assert.Equal(expectedPricePerNightAfterDiscount, roomWithDiscount.PricePerNightAfterDiscount);
        Assert.Equal(expectedTotalRoomPrice, roomWithDiscount.TotalRoomPrice);
        Assert.Equal(expectedTotalRoomPriceAfterDiscount, roomWithDiscount.TotalRoomPriceAfterDiscount);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectRricesPerRoom_WhenBookingRoomWithDiscount()
    {
        // Arrange
        string userId = "jgjy-kgjg-lkyf";
        Booking booking = CreateBooking();
        int numberOfNights = 5;
        decimal expectedPricePerNight = booking.BookingRooms.ToList()[1].Room.Price;
        decimal expectedPricePerNightAfterDiscount = booking.BookingRooms.ToList()[1].FinalPrice;
        decimal expectedTotalRoomPrice = expectedPricePerNight * numberOfNights;
        decimal expectedTotalRoomPriceAfterDiscount = expectedPricePerNightAfterDiscount * numberOfNights;
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(booking.Id)).ReturnsAsync(booking);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(userId)).ReturnsAsync(booking.Guest);
        _currentUserMock.Setup(x => x.Id).Returns(userId);

        // Act 
        InvoiceDto? result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        HotelBookingPlatform.Application.DTOs.Room.RoomWithinInvoiceDto roomWithoutDiscount = result.Rooms[1];
        Assert.NotNull(roomWithoutDiscount);
        Assert.Equal(expectedPricePerNight, roomWithoutDiscount.PricePerNight);
        Assert.Equal(expectedPricePerNightAfterDiscount, roomWithoutDiscount.PricePerNightAfterDiscount);
        Assert.Equal(expectedTotalRoomPrice, roomWithoutDiscount.TotalRoomPrice);
        Assert.Equal(expectedTotalRoomPriceAfterDiscount, roomWithoutDiscount.TotalRoomPriceAfterDiscount);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldThrowUnauthorizedException_WhenCurrentUserIsNotGuestOfBooking()
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
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.GetInvoiceAsync(booking.Id));
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        // Arrange
        Guid bookingId = Guid.NewGuid();
        _bookingRepositoryMock.Setup(x => x.GetBookingAsync(bookingId)).ReturnsAsync((Booking?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetInvoiceAsync(bookingId));
        _bookingRepositoryMock.Verify(x => x.GetBookingAsync(bookingId), Times.Once);
    }
}


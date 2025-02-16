using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Email;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Pdf;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Application.Validators.Bookings;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests.BookingServiceTests;

public class BookingServiceInvoiceCalculationsTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IGuestService> _guestServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<BookingService>> _loggerMock;
    private readonly IInvoiceService _invoiceServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEntityValidatorService> _entityValidatorServiceMock;
    private readonly BookingServiceValidator _bookingValidator;
    private readonly IMapper _mapper;
    private readonly IFixture _fixture;
    private readonly BookingService sut;

    public BookingServiceInvoiceCalculationsTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _guestServiceMock = new Mock<IGuestService>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<BookingService>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _entityValidatorServiceMock = new Mock<IEntityValidatorService>();
        _fixture = FixtureFactory.CreateFixture();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _invoiceServiceMock = new InvoiceService(_mapper, new Mock<IPdfGenerator>().Object, new Mock<ILogger<InvoiceService>>().Object);
        var validatorLoggerMock = new Mock<ILogger<BookingServiceValidator>>();
        _bookingValidator = new BookingServiceValidator(
            _roomRepositoryMock.Object,
            validatorLoggerMock.Object
        );

        sut = new BookingService(
            _roomRepositoryMock.Object,
            _bookingRepositoryMock.Object,
            _mapper,
            _guestServiceMock.Object,
            _emailServiceMock.Object,
            _invoiceServiceMock,
            _loggerMock.Object,
            _bookingValidator,
            _entityValidatorServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }
    private Booking CreateBooking()
    {
        var guest = _fixture.Build<Guest>()
                   .Without(g => g.Bookings)
                   .Create();

        var roomWithDiscount = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.Discounts)
                   .Without(r => r.Hotel)
                   .Without(r => r.BookingRooms)
                   .With(r => r.Price, 200)
                   .Without(r => r.Amenities)
                   .Create();
        var discount = _fixture.Build<Discount>()
                   .With(d => d.Room, roomWithDiscount)
                   .With(d => d.RoomId, roomWithDiscount.Id)
                   .With(d => d.Percentage, 10)
                   .Create();
        var bookingRoomWithDiscount = _fixture.Build<BookingRoom>()
              .With(d => d.Room, roomWithDiscount)
              .With(d => d.RoomId, roomWithDiscount.Id)
              .Without(r => r.Booking)
              .Without(r => r.BookingId)
              .With(r => r.DiscountId, discount.Id)
              .With(r => r.Discount, discount)
              .Without(r => r.FinalPrice)
              .Create();

        var roomWithoutDiscount = _fixture.Build<Room>()
                  .Without(r => r.Images)
                  .Without(r => r.Discounts)
                  .Without(r => r.Hotel)
                  .Without(r => r.BookingRooms)
                  .With(r => r.Price, 500)
                  .Without(r => r.Amenities)
                  .Create();
        var bookingRoomWithoutDiscount = _fixture.Build<BookingRoom>()
              .With(d => d.Room, roomWithoutDiscount)
              .With(d => d.RoomId, roomWithoutDiscount.Id)
              .Without(r => r.Booking)
              .Without(r => r.BookingId)
              .Without(r => r.DiscountId)
              .Without(r => r.Discount)
              .Without(r => r.FinalPrice)
              .Create();

        var booking = _fixture.Build<Booking>()
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
        var invoice = _fixture.Create<InvoiceDto>();
        var userId = "jgjy-kgjg-lkyf";
        var booking = CreateBooking();
        var currentGuestDto = _fixture.Build<CurrentGuestDto>()
            .With(c => c.Guest, booking.Guest)
            .Create();
        _guestServiceMock.Setup(x => x.GetGuestFromCurrentUser()).ReturnsAsync(currentGuestDto);
        _entityValidatorServiceMock
           .Setup(x => x.ValidateBookingAsync(booking.Id))
           .ReturnsAsync(booking);

        // Act
        var result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InvoiceDto>(result);
        _entityValidatorServiceMock.Verify(x => x.ValidateBookingAsync(booking.Id), Times.Once);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectNumberOfNightsInTheReturnedInvoice_WhenBookingRooms()
    {
        // Arrange
        var userId = "jgjy-kgjg-lkyf";
        var booking = CreateBooking();
        var invoice = _fixture.Create<InvoiceDto>();
        var expectedNumberOfNights = 5;
        var currentGuestDto = _fixture.Build<CurrentGuestDto>()
           .With(c => c.Guest, booking.Guest)
           .Create();
        _guestServiceMock.Setup(x => x.GetGuestFromCurrentUser()).ReturnsAsync(currentGuestDto);
        _entityValidatorServiceMock
           .Setup(x => x.ValidateBookingAsync(booking.Id))
           .ReturnsAsync(booking);

        // Act
        var result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        var firstRoomInvoice = result.Rooms.First();
        Assert.Equal(expectedNumberOfNights, firstRoomInvoice.NumberOfNights);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectTotalPricesInTheReturnedInvoice_WhenBookingRooms()
    {
        // Arrange
        var userId = "jgjy-kgjg-lkyf";
        var booking = CreateBooking();
        var numberOfNights = 5;
        var expectedTotalRoomsPrice = booking.BookingRooms.Sum(r => r.Room.Price * numberOfNights);
        var expectedTotalRoomsPriceAfterDiscounts = booking.BookingRooms.Sum(r => r.FinalPrice * numberOfNights);
        var currentGuestDto = _fixture.Build<CurrentGuestDto>()
           .With(c => c.Guest, booking.Guest)
           .Create();
        _guestServiceMock.Setup(x => x.GetGuestFromCurrentUser()).ReturnsAsync(currentGuestDto);
        _entityValidatorServiceMock
           .Setup(x => x.ValidateBookingAsync(booking.Id))
           .ReturnsAsync(booking);

        // Act & Assert
        var result = await sut.GetInvoiceAsync(booking.Id);
        Assert.NotNull(result);
        Assert.Equal(expectedTotalRoomsPrice, result.TotalPrice);
        Assert.Equal(expectedTotalRoomsPriceAfterDiscounts, result.TotalPriceAfterDiscount);
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldCalculateCorrectRricesPerRoom_WhenBookingRoomWithoutDiscount()
    {
        // Arrange
        var userId = "jgjy-kgjg-lkyf";
        var booking = CreateBooking();
        var numberOfNights = 5;
        var expectedPricePerNight = booking.BookingRooms.ToList()[0].Room.Price;
        var expectedPricePerNightAfterDiscount = booking.BookingRooms.ToList()[0].FinalPrice;
        var expectedTotalRoomPrice = expectedPricePerNight * numberOfNights;
        var expectedTotalRoomPriceAfterDiscount = expectedPricePerNightAfterDiscount * numberOfNights;
        var currentGuestDto = _fixture.Build<CurrentGuestDto>()
           .With(c => c.Guest, booking.Guest)
           .Create();
        _guestServiceMock.Setup(x => x.GetGuestFromCurrentUser()).ReturnsAsync(currentGuestDto);
        _entityValidatorServiceMock
           .Setup(x => x.ValidateBookingAsync(booking.Id))
           .ReturnsAsync(booking);

        // Act 
        var result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        var roomWithDiscount = result.Rooms[0];
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
        var userId = "jgjy-kgjg-lkyf";
        var booking = CreateBooking();
        var numberOfNights = 5;
        var expectedPricePerNight = booking.BookingRooms.ToList()[1].Room.Price;
        var expectedPricePerNightAfterDiscount = booking.BookingRooms.ToList()[1].FinalPrice;
        var expectedTotalRoomPrice = expectedPricePerNight * numberOfNights;
        var expectedTotalRoomPriceAfterDiscount = expectedPricePerNightAfterDiscount * numberOfNights;
        var currentGuestDto = _fixture.Build<CurrentGuestDto>()
           .With(c => c.Guest, booking.Guest)
           .Create();
        _guestServiceMock.Setup(x => x.GetGuestFromCurrentUser()).ReturnsAsync(currentGuestDto);
        _entityValidatorServiceMock
           .Setup(x => x.ValidateBookingAsync(booking.Id))
           .ReturnsAsync(booking);

        // Act 
        var result = await sut.GetInvoiceAsync(booking.Id);

        // Assert
        Assert.NotNull(result);
        var roomWithoutDiscount = result.Rooms[1];
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
        var unauthorizedGuest = _fixture.Build<Guest>()
            .Without(g => g.Bookings)
            .Create();
        var booking = _fixture.Build<Booking>()
            .Without(b => b.BookingRooms)
            .Without(b => b.Hotel)
            .Without(b => b.Guest)
            .Without(b => b.GuestId)// different GuestId than the current user id 
            .Create();
        var unauthorizedUserId = "kjhkhf-kjenkjg-kjngk";
        var currentGuestDto = _fixture.Build<CurrentGuestDto>()
           .With(c => c.Guest, booking.Guest)
           .Create();
        _guestServiceMock.Setup(x => x.GetGuestFromCurrentUser())
            .ThrowsAsync(new UnauthorizedException(currentGuestDto.UserId, nameof(Booking), unauthorizedGuest.Id));
        _entityValidatorServiceMock
           .Setup(x => x.ValidateBookingAsync(booking.Id))
           .ReturnsAsync(booking);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.GetInvoiceAsync(booking.Id));
    }

    [Fact]
    public async Task GetInvoiceAsync_ShouldThrowNotFoundException_WhenBookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        _entityValidatorServiceMock
          .Setup(x => x.ValidateBookingAsync(bookingId))
          .ThrowsAsync(new NotFoundException(nameof(Booking), bookingId));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetInvoiceAsync(bookingId));
        _entityValidatorServiceMock.Verify(x => x.ValidateBookingAsync(bookingId), Times.Once);
    }
}


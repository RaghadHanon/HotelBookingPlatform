using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests;

public class GuestServiceTests
{
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IBookingRepository> _bookingRepositoryMock;
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<GuestService>> _loggerMock;
    private readonly IFixture _fixture;
    private readonly GuestService sut;

    public GuestServiceTests()
    {
        _fixture = FixtureFactory.CreateFixture();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _bookingRepositoryMock = new Mock<IBookingRepository>();
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _loggerMock = new Mock<ILogger<GuestService>>();

        sut = new GuestService(_guestRepositoryMock.Object,
                               _bookingRepositoryMock.Object,
                               _hotelRepositoryMock.Object,
                               _mapper,
                               _currentUserMock.Object,
                               _loggerMock.Object);
    }

    [Theory]
    [InlineData(1, 5, 5, false, false)]
    [InlineData(1, 10, 30, false, true)]
    [InlineData(2, 10, 30, true, true)]
    [InlineData(3, 10, 30, true, false)]
    public async Task GetAllBookingsForGuestAsync_ShouldHandlePaginationCorrectly(int pageNumber, int pageSize, int totalCount, bool HasPreviousPage, bool HasNextPage)
    {
        // Arrange
        GetBookingsQueryParametersDto requestParameters = new GetBookingsQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        List<Booking> expectedBookings = _fixture.Build<Booking>()
             .Without(b => b.BookingRooms)
             .Without(b => b.Hotel)
             .With(b => b.Guest, guest)
             .CreateMany(totalCount)
             .ToList();
        PaginationMetadata expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        int skippedBookings = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        PaginatedResult<Booking> expectedPaginatedResult = new PaginatedResult<Booking>(expectedBookings.Skip(skippedBookings).Take(pageSize).ToList(), expectedPaginationMetadata);
        _guestRepositoryMock.Setup(repo => repo.GuestExistsAsync(guest.Id)).ReturnsAsync(true);
        _guestRepositoryMock.Setup(repo => repo.GetAllBookingsForGuestAsync(guest.Id, requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        PaginatedResult<BookingForGuestOutputDto> paginatedResult = await sut.GetAllBookingsForGuestAsync(guest.Id, requestParameters);

        // Assert
        _guestRepositoryMock.Verify(c => c.GuestExistsAsync(guest.Id), Times.Once);
        _guestRepositoryMock.Verify(c => c.GetAllBookingsForGuestAsync(guest.Id, requestParameters), Times.Once);
        Assert.Equal(pageSize, paginatedResult.Data.Count());
        Assert.Equal(pageNumber, paginatedResult.Metadata.PageNumber);
        Assert.Equal(pageSize, paginatedResult.Metadata.PageSize);
        Assert.Equal(totalCount, paginatedResult.Metadata.TotalCount);
        Assert.Equal(HasPreviousPage, paginatedResult.Metadata.HasPreviousPage);
        Assert.Equal(HasNextPage, paginatedResult.Metadata.HasNextPage);
    }

    [Fact]
    public async Task GetRecentlyVisitedHotelsAsync_ShouldReturnHotels_WhenValidGuestIdAndCount()
    {
        // Arrange
        int count = 5;
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        IEnumerable<Booking> recentBookings = _fixture.Build<Booking>()
             .Without(b => b.BookingRooms)
             .Without(b => b.Hotel)
             .With(b => b.Guest, guest)
             .CreateMany(count);
        _guestRepositoryMock.Setup(x => x.GuestExistsAsync(guest.Id)).ReturnsAsync(true);
        _guestRepositoryMock.Setup(x => x.GetRecentBookingsInDifferentHotelsAsync(guest.Id, count)).ReturnsAsync(recentBookings);

        // Act
        IEnumerable<RecentlyVisitedHotelOutputDto> result = await sut.GetRecentlyVisitedHotelsAsync(guest.Id, count);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<RecentlyVisitedHotelOutputDto>>(result);
        Assert.Equal(count, result.Count());
        _guestRepositoryMock.Verify(x => x.GetRecentBookingsInDifferentHotelsAsync(guest.Id, count), Times.Once);
    }

    [Fact]
    public async Task GetRecentlyVisitedHotelsAsync_ShouldThrowNotFoundException_WhenGuestDoesNotExist()
    {
        // Arrange
        int count = 5;
        _guestRepositoryMock.Setup(x => x.GuestExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetRecentlyVisitedHotelsAsync(It.IsAny<Guid>(), count));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetRecentlyVisitedHotelsAsync_ShouldThrowBadRequestException_WhenInvalidCount(int count)
    {
        // Arrange
        _guestRepositoryMock.Setup(x => x.GuestExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.GetRecentlyVisitedHotelsAsync(It.IsAny<Guid>(), count));
    }

    [Fact]
    public async Task GetRecentlyVisitedHotelsForCurrentUserAsync_ShouldReturnHotels_WhenValidCount()
    {
        // Arrange
        int count = 5;
        string currentUserId = "ghsdk-lkjhs-557sl";
        Guest guest = _fixture.Build<Guest>()
                   .Without(g => g.Bookings)
                   .Create();
        IEnumerable<Booking> recentBookings = _fixture.Build<Booking>()
                            .Without(b => b.BookingRooms)
                            .Without(b => b.Hotel)
                            .With(b => b.Guest, guest)
                            .CreateMany(count);

        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync(guest);
        _guestRepositoryMock.Setup(x => x.GetRecentBookingsInDifferentHotelsAsync(guest.Id, count)).ReturnsAsync(recentBookings);

        // Act
        IEnumerable<RecentlyVisitedHotelOutputDto> result = await sut.GetRecentlyVisitedHotelsForCurrentUserAsync(count);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<RecentlyVisitedHotelOutputDto>>(result);
        Assert.Equal(count, result.Count());
        _guestRepositoryMock.Verify(x => x.GetGuestByUserIdAsync(currentUserId), Times.Once);
        _guestRepositoryMock.Verify(x => x.GetRecentBookingsInDifferentHotelsAsync(guest.Id, count), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetRecentlyVisitedHotelsForCurrentUserAsync_ShouldThrowBadRequestException_WhenInvalidCount(int count)
    {
        // Arrange
        string userId = "current_user_id";
        _currentUserMock.Setup(x => x.Id).Returns(userId);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.GetRecentlyVisitedHotelsForCurrentUserAsync(count));
    }

    [Fact]
    public async Task GetRecentlyVisitedHotelsForCurrentUserAsync_ShouldThrowNotFoundException_WhenGuestDoesNotExist()
    {
        // Arrange
        int count = 5;
        string currentUserId = "ghsdk-lkjhs-557sl";
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync((Guest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetRecentlyVisitedHotelsForCurrentUserAsync(count));
    }
}

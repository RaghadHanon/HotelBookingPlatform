using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests;

public class DiscountServiceTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IDiscountRepository> _discountRepositoryMock;
    private readonly Mock<ILogger<DiscountService>> _loggerMock;
    private readonly DiscountService sut;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;

    public DiscountServiceTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _discountRepositoryMock = new Mock<IDiscountRepository>();
        _loggerMock = new Mock<ILogger<DiscountService>>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        sut = new DiscountService(_roomRepositoryMock.Object, _discountRepositoryMock.Object, _mapper, _loggerMock.Object);
        _fixture = FixtureFactory.CreateFixture();
    }

    [Theory]
    [InlineData(1, 5, 5, false, false)]
    [InlineData(1, 10, 30, false, true)]
    [InlineData(2, 10, 30, true, true)]
    [InlineData(3, 10, 30, true, false)]
    public async Task GetDiscountsForRoomAsync_ShouldHandlePaginationCorrectly(int pageNumber, int pageSize, int totalCount, bool HasPreviousPage, bool HasNextPage)
    {
        // Arrange
        GetDiscountsQueryParametersDto requestParameters = new GetDiscountsQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        List<Discount> expectedDiscounts = _fixture.Build<Discount>()
            .Without(d => d.Room)
            .Without(d => d.RoomId)
            .CreateMany(totalCount)
            .ToList();
        PaginationMetadata expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        int skippedDiscountes = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        PaginatedResult<Discount> expectedPaginatedResult = new PaginatedResult<Discount>(expectedDiscounts.Skip(skippedDiscountes).Take(pageSize).ToList(), expectedPaginationMetadata);
        _discountRepositoryMock.Setup(repo => repo.GetDiscountsForRoomAsync(It.IsAny<Guid>(), requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        PaginatedResult<DiscountForRoomOutputDto> paginatedResult = await sut.GetDiscountsForRoomAsync(It.IsAny<Guid>(), requestParameters);

        // Assert
        _discountRepositoryMock.Verify(c => c.GetDiscountsForRoomAsync(It.IsAny<Guid>(), requestParameters), Times.Once);
        Assert.Equal(pageSize, paginatedResult.Data.Count());
        Assert.Equal(pageNumber, paginatedResult.Metadata.PageNumber);
        Assert.Equal(pageSize, paginatedResult.Metadata.PageSize);
        Assert.Equal(totalCount, paginatedResult.Metadata.TotalCount);
        Assert.Equal(HasPreviousPage, paginatedResult.Metadata.HasPreviousPage);
        Assert.Equal(HasNextPage, paginatedResult.Metadata.HasNextPage);
    }

    [Fact]
    public async Task DeleteDiscountAsync_ShouldReturnTrue_WhenDiscountDeleted()
    {
        // Arrange
        _discountRepositoryMock.Setup(x => x.DeleteDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        bool result = await sut.DeleteDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>());

        // Assert
        Assert.True(result);
        _discountRepositoryMock.Verify(x => x.DeleteDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        _discountRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteDiscountAsync_ShouldReturnFalse_WhenDiscountNotDeleted()
    {
        // Arrange
        _discountRepositoryMock.Setup(x => x.DeleteDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        bool result = await sut.DeleteDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>());

        // Assert
        Assert.False(result);
        _discountRepositoryMock.Verify(x => x.DeleteDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
        _discountRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetDiscountAsync_ShouldReturnDiscount_WhenDiscountExists()
    {
        // Arrange
        Discount discount = _fixture.Build<Discount>()
            .Without(d => d.Room)
            .Without(d => d.RoomId)
            .Create();
        _discountRepositoryMock.Setup(x => x.GetDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(discount);

        // Act
        DiscountOutputDto? result = await sut.GetDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>());

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DiscountOutputDto>(result);
        _discountRepositoryMock.Verify(x => x.GetDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetDiscountAsync_ShouldThrowNotFoundException_WhenDiscountDoesNotExist()
    {
        // Arrange
        _discountRepositoryMock.Setup(x => x.GetDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((Discount?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>()));
        _discountRepositoryMock.Verify(x => x.GetDiscountAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task GetFeaturedDealsAsync_ShouldReturnFeaturedDeals_WhenDealsExist()
    {
        // Arrange
        int dealsCount = 2;
        List<Room> rooms = _fixture.Build<Room>()
                    .Without(r => r.Images)
                    .Without(r => r.Hotel)
                    .Without(r => r.BookingRooms)
                    .Without(r => r.Amenities)
                    .With(r => r.Discounts, [_fixture.Build<Discount>()
                                             .Without(d => d.Room)
                                             .Without(d => d.RoomId)
                                             .Create()
                                             ])
                    .CreateMany(2)
                    .ToList();
        _roomRepositoryMock.Setup(x => x.GetRoomsWithMostRecentHighestDiscounts(dealsCount)).ReturnsAsync(rooms);

        // Act
        IEnumerable<HotelBookingPlatform.Application.DTOs.Hotel.FeaturedDealOutputDto> result = await sut.GetFeaturedDealsAsync(dealsCount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dealsCount, result.Count());
        _roomRepositoryMock.Verify(x => x.GetRoomsWithMostRecentHighestDiscounts(dealsCount), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(21)]
    public async Task GetFeaturedDealsAsync_ShouldThrowBadRequestException_WhenInvalidDealsCount(
        int invalidDeals)
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.GetFeaturedDealsAsync(invalidDeals));
        _roomRepositoryMock.Verify(x => x.GetRoomsWithMostRecentHighestDiscounts(It.IsAny<int>()), Times.Never);
    }


    private CreateDiscountDto GetValidCreateDiscountDto()
    {
        return _fixture.Build<CreateDiscountDto>()
            .With(x => x.StartDate, DateTime.UtcNow.AddDays(1))
            .With(x => x.EndDate, DateTime.UtcNow.AddDays(4))
            .Create();
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldCreateDiscount_WhenValidDataProvided()
    {
        // Arrange
        CreateDiscountDto validCreateDiscountDto = GetValidCreateDiscountDto();
        validCreateDiscountDto.DiscountedPrice = 120m;
        Room room = _fixture.Build<Room>()
                    .Without(r => r.Images)
                    .Without(r => r.Hotel)
                    .Without(r => r.BookingRooms)
                    .Without(r => r.Discounts)
                    .Without(r => r.Amenities)
                    .With(x => x.Price, 200)
                    .Create();
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);

        // Act
        DiscountOutputDto result = await sut.CreateDiscountAsync(room.Id, validCreateDiscountDto);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DiscountOutputDto>(result);
        _roomRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldReturnCorrectDiscountData_WhenDiscountedPriceProvided()
    {
        // Arrange
        Room room = _fixture.Build<Room>()
                    .Without(r => r.Images)
                    .Without(r => r.Hotel)
                    .Without(r => r.BookingRooms)
                    .Without(r => r.Discounts)
                    .Without(r => r.Amenities)
                    .With(x => x.Price, 150m)
                    .Create();
        decimal discountedPrice = 105m;
        CreateDiscountDto validCreateDiscountDto = GetValidCreateDiscountDto();
        validCreateDiscountDto.DiscountedPrice = discountedPrice;
        decimal expectedPercentage = (room.Price - discountedPrice) / room.Price * 100; // 30%
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);

        // Act
        DiscountOutputDto result = await sut.CreateDiscountAsync(room.Id, validCreateDiscountDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(discountedPrice, result.DiscountedPrice);
        Assert.Equal(room.Price, result.OriginalPrice);
        Assert.Equal(expectedPercentage, result.Percentage);
        _roomRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldReturnCorrectDiscountDataAndIgnorePercentage_WhenDiscountedPriceAndPercentageProvided()
    {
        // Arrange
        Room room = _fixture.Build<Room>()
                    .Without(r => r.Images)
                    .Without(r => r.Hotel)
                    .Without(r => r.BookingRooms)
                    .Without(r => r.Discounts)
                    .Without(r => r.Amenities)
                    .With(x => x.Price, 150m)
                    .Create();
        decimal discountedPrice = 105m;
        CreateDiscountDto validCreateDiscountDto = GetValidCreateDiscountDto();
        //both percentage and discounted price are provided
        validCreateDiscountDto.DiscountedPrice = discountedPrice;
        validCreateDiscountDto.Percentage = 10m;
        decimal expectedPercentage = (room.Price - discountedPrice) / room.Price * 100; // 30% 
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);

        // Act
        DiscountOutputDto result = await sut.CreateDiscountAsync(room.Id, validCreateDiscountDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(discountedPrice, result.DiscountedPrice);
        Assert.Equal(room.Price, result.OriginalPrice);
        Assert.Equal(expectedPercentage, result.Percentage);
        _roomRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldReturnCorrectDiscountData_WhenPercentageProvided()
    {
        // Arrange
        Room room = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.Hotel)
                   .Without(r => r.BookingRooms)
                   .Without(r => r.Discounts)
                   .Without(r => r.Amenities)
                   .With(x => x.Price, 150m)
                   .Create();
        decimal percentage = 30m;
        CreateDiscountDto validCreateDiscountDto = GetValidCreateDiscountDto();
        validCreateDiscountDto.Percentage = percentage;
        validCreateDiscountDto.DiscountedPrice = null;
        decimal expectedRoundedDiscountedPrice = Math.Round(room.Price - (room.Price * percentage / 100), 2); // 105.00
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);

        // Act
        DiscountOutputDto result = await sut.CreateDiscountAsync(room.Id, validCreateDiscountDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(room.Id, result.RoomId);
        Assert.Equal(percentage, result.Percentage);
        Assert.Equal(room.Price, result.OriginalPrice);
        Assert.Equal(expectedRoundedDiscountedPrice, result.DiscountedPrice);
        _roomRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldThrowBadRequestException_WhenNoDiscountDataProvided()
    {
        // Arrange
        Guid roomId = Guid.NewGuid();
        CreateDiscountDto validCreateDiscountDto = GetValidCreateDiscountDto();
        validCreateDiscountDto.DiscountedPrice = null;
        validCreateDiscountDto.Percentage = null;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.CreateDiscountAsync(roomId, validCreateDiscountDto));
    }

    [Fact]
    public async Task CreateDiscountAsync_ShouldThrowNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange
        Guid roomId = Guid.NewGuid();
        CreateDiscountDto validCreateDiscountDto = GetValidCreateDiscountDto();
        _roomRepositoryMock.Setup(x => x.GetRoomAsync(roomId)).ReturnsAsync((Room?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateDiscountAsync(roomId, validCreateDiscountDto));
    }

    [Theory]
    [InlineData(150, 105, 30)]
    [InlineData(200, 160, 20)]
    [InlineData(300, 150, 50)]
    [InlineData(400, 340, 15)]
    public void DiscountConstructor_ShouldSetCorrectPercentage(decimal originalPrice, decimal discountedPrice, decimal expectedPercentage)
    {
        // Arrange
        Room room = _fixture.Build<Room>()
                   .Without(r => r.Images)
                   .Without(r => r.Hotel)
                   .Without(r => r.BookingRooms)
                   .Without(r => r.Discounts)
                   .With(x => x.Price, originalPrice)
                   .Without(r => r.Amenities)
                   .Create();
        DateTime startDate = DateTime.UtcNow.AddDays(1);
        DateTime endDate = DateTime.UtcNow.AddDays(6);

        // Act
        Discount discount = new Discount(room, originalPrice, discountedPrice, startDate, endDate);

        // Assert
        Assert.Equal(expectedPercentage, discount.Percentage);
    }
}

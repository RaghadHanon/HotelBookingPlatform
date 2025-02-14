using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests;

public class ReviewServiceTests
{
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly Mock<IGuestRepository> _guestRepositoryMock;
    private readonly Mock<IReviewRepository> _reviewRepositoryMock;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ReviewService>> _loggerMock;
    private readonly ReviewService sut;

    public ReviewServiceTests()
    {
        _fixture = FixtureFactory.CreateFixture();
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _guestRepositoryMock = new Mock<IGuestRepository>();
        _reviewRepositoryMock = new Mock<IReviewRepository>();
        _currentUserMock = new Mock<ICurrentUser>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _loggerMock = new Mock<ILogger<ReviewService>>();
        sut = new ReviewService(_hotelRepositoryMock.Object, _guestRepositoryMock.Object,
                                _reviewRepositoryMock.Object, _mapper, _currentUserMock.Object, _loggerMock.Object);
    }


    [Theory]
    [InlineData(1, 5, 5, false, false)]
    [InlineData(1, 10, 30, false, true)]
    [InlineData(2, 10, 30, true, true)]
    [InlineData(3, 10, 30, true, false)]
    public async Task GetHotelReviewsAsync_ShouldHandlePaginationCorrectly(int pageNumber, int pageSize, int totalCount, bool HasPreviousPage, bool HasNextPage)
    {
        // Arrange
        GetHotelReviewsQueryParameters requestParameters = new GetHotelReviewsQueryParameters { PageNumber = pageNumber, PageSize = pageSize };
        List<Review> expectedReviews = _fixture.Build<Review>()
             .Without(r => r.Guest)
             .Without(r => r.GuestId)
             .Without(r => r.Hotel)
             .Without(r => r.HotelId)
             .CreateMany(totalCount)
             .ToList();
        Hotel hotel = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .Create();
        PaginationMetadata expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        int skippedReviews = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        PaginatedResult<Review> expectedPaginatedResult = new PaginatedResult<Review>(expectedReviews.Skip(skippedReviews).Take(pageSize).ToList(), expectedPaginationMetadata);

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _reviewRepositoryMock.Setup(x => x.GetHotelReviewsAsync(hotel, requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        PaginatedResult<ReviewOutputDto> paginatedResult = await sut.GetHotelReviewsAsync(hotel.Id, requestParameters);

        // Assert
        _reviewRepositoryMock.Verify(r => r.GetHotelReviewsAsync(hotel, requestParameters), Times.Once);
        Assert.IsAssignableFrom<IEnumerable<ReviewOutputDto>>(paginatedResult.Data);
        Assert.IsType<PaginationMetadata>(paginatedResult.Metadata);
        Assert.Equal(pageSize, paginatedResult.Data.Count());
        Assert.Equal(pageNumber, paginatedResult.Metadata.PageNumber);
        Assert.Equal(pageSize, paginatedResult.Metadata.PageSize);
        Assert.Equal(totalCount, paginatedResult.Metadata.TotalCount);
        Assert.Equal(HasPreviousPage, paginatedResult.Metadata.HasPreviousPage);
        Assert.Equal(HasNextPage, paginatedResult.Metadata.HasNextPage);
    }

    [Fact]
    public async Task GetHotelReviewsAsync_ShouldThrowNotFoundException_WhenHotelDoesNotExist()
    {
        // Arrange
        Guid hotelId = Guid.NewGuid();
        GetHotelReviewsQueryParameters request = new GetHotelReviewsQueryParameters();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotelId)).ReturnsAsync((Hotel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetHotelReviewsAsync(hotelId, request));
    }

    [Fact]
    public async Task AddReviewAsync_ShouldCreateReview_WhenHotelExistsAndGuestIsAuthorized()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync(guest);
        _guestRepositoryMock.Setup(x => x.HasGuestBookedHotelAsync(hotel, guest)).ReturnsAsync(true);
        _guestRepositoryMock.Setup(x => x.HasGuestReviewedHotelAsync(hotel, guest)).ReturnsAsync(false);

        // Act
        ReviewOutputDto result = await sut.AddReviewAsync(hotel.Id, createOrUpdateReviewDto);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ReviewOutputDto>(result);
        Assert.Equal(hotel.Id, result.HotelId);
        _reviewRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _reviewRepositoryMock.Verify(r => r.AddReviewAsync(hotel, It.IsAny<Review>()), Times.Once);
    }

    [Fact]
    public async Task AddReviewAsync_ShouldThrowBadRequestException_WhenGuestHasNotBookedTheHotel()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync(guest);
        _guestRepositoryMock.Setup(x => x.HasGuestBookedHotelAsync(hotel, guest)).ReturnsAsync(false);
        _guestRepositoryMock.Setup(x => x.HasGuestReviewedHotelAsync(hotel, guest)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.AddReviewAsync(hotel.Id, createOrUpdateReviewDto));
    }

    [Fact]
    public async Task AddReviewAsync_ShouldThrowBadRequestException_WhenGuestHasAlreadyReviewedHotel()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync(guest);
        _guestRepositoryMock.Setup(x => x.HasGuestBookedHotelAsync(hotel, guest)).ReturnsAsync(true);
        _guestRepositoryMock.Setup(x => x.HasGuestReviewedHotelAsync(hotel, guest)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.AddReviewAsync(hotel.Id, createOrUpdateReviewDto));
    }

    [Fact]
    public async Task AddReviewAsync_ShouldThrowNotFoundException_WhenHotelDoesNotExist()
    {
        // Arrange
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(It.IsAny<Guid>())).ReturnsAsync((Hotel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.AddReviewAsync(It.IsAny<Guid>(), createOrUpdateReviewDto));
    }

    [Fact]
    public async Task AddReviewAsync_ShouldThrowUnauthenticatedException_WhenGuestDoesNotExist()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync((Guest?)null);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthenticatedException>(() => sut.AddReviewAsync(hotel.Id, createOrUpdateReviewDto));
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldUpdateReview_WhenGuestIsAuthorized()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        Review review = _fixture.Build<Review>()
             .Without(r => r.Guest)
             .Without(r => r.GuestId)
             .Without(r => r.Hotel)
             .Without(r => r.HotelId)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(new Hotel());
        _reviewRepositoryMock.Setup(x => x.GetReviewAsync(It.IsAny<Hotel>(), review.Id)).ReturnsAsync(review);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync(guest);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        review.GuestId = guest.Id;

        // Act
        bool result = await sut.UpdateReviewAsync(hotel.Id, review.Id, createOrUpdateReviewDto);

        // Assert
        Assert.True(result);
        _reviewRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldThrowNotFoundException_WhenReviewDoesNotExist()
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
        Review review = _fixture.Build<Review>()
             .Without(r => r.Guest)
             .Without(r => r.GuestId)
             .Without(r => r.Hotel)
             .Without(r => r.HotelId)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(new Hotel());
        _reviewRepositoryMock.Setup(x => x.GetReviewAsync(It.IsAny<Hotel>(), review.Id)).ReturnsAsync((Review?)null);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateReviewAsync(hotel.Id, review.Id, createOrUpdateReviewDto));
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldThrowUnauthorizedException_WhenUserIsNotAuthorized()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        Review review = _fixture.Build<Review>()
             .Without(r => r.Guest)
             .Without(r => r.GuestId)
             .Without(r => r.Hotel)
             .Without(r => r.HotelId)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(new Hotel());
        _reviewRepositoryMock.Setup(x => x.GetReviewAsync(It.IsAny<Hotel>(), review.Id)).ReturnsAsync(review);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync(guest);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        review.GuestId = Guid.NewGuid(); // To simulate unauthorized user

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => sut.UpdateReviewAsync(hotel.Id, review.Id, createOrUpdateReviewDto));
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldThrowUnauthenticatedException_WhenGuestDoesNotExist()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        Review review = _fixture.Build<Review>()
             .Without(r => r.Guest)
             .Without(r => r.GuestId)
             .Without(r => r.Hotel)
             .Without(r => r.HotelId)
             .Create();
        string currentUserId = "ghsdk-lkjhs-557sl";
        CreateOrUpdateReviewDto createOrUpdateReviewDto = _fixture.Create<CreateOrUpdateReviewDto>();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(new Hotel());
        _reviewRepositoryMock.Setup(x => x.GetReviewAsync(It.IsAny<Hotel>(), review.Id)).ReturnsAsync(review);
        _currentUserMock.Setup(x => x.Id).Returns(currentUserId);
        _guestRepositoryMock.Setup(x => x.GetGuestByUserIdAsync(currentUserId)).ReturnsAsync((Guest?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthenticatedException>(() => sut.AddReviewAsync(hotel.Id, createOrUpdateReviewDto));
    }

    [Fact]
    public async Task GetReviewAsync_ShouldReturnReview_WhenReviewExists()
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
        Guest guest = _fixture.Build<Guest>()
             .Without(g => g.Bookings)
             .Create();
        Review review = _fixture.Build<Review>()
             .Without(r => r.Guest)
             .Without(r => r.GuestId)
             .Without(r => r.Hotel)
             .Without(r => r.HotelId)
             .Create();

        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _reviewRepositoryMock.Setup(x => x.GetReviewAsync(hotel, review.Id)).ReturnsAsync(review);

        // Act
        ReviewOutputDto result = await sut.GetReviewAsync(hotel.Id, review.Id);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ReviewOutputDto>(result);
    }

    [Fact]
    public async Task GetReviewAsync_ShouldThrowNotFoundException_WhenReviewDoesNotExist()
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
        Guid reviewId = Guid.NewGuid();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _reviewRepositoryMock.Setup(x => x.GetReviewAsync(hotel, reviewId)).ReturnsAsync((Review?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetReviewAsync(hotel.Id, reviewId));
    }

    [Fact]
    public async Task GetReviewAsync_ShouldThrowNotFoundException_WhenHotelDoesNotExist()
    {
        // Arrange
        Guid hotelId = Guid.NewGuid();
        Guid reviewId = Guid.NewGuid();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotelId)).ReturnsAsync((Hotel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetReviewAsync(hotelId, reviewId));
    }

    [Fact]
    public async Task GetHotelAverageRatingAsync_ShouldReturnRoundedAverageRating_WhenHotelExists()
    {
        // Arrange
        double expectedRating = 3.8;
        double roundedExpectedRating = Math.Round(expectedRating, 1);
        Hotel hotel = _fixture.Build<Hotel>()
               .Without(r => r.Images)
               .Without(r => r.City)
               .Without(r => r.Location)
               .Without(r => r.Rooms)
               .Without(r => r.Reviews)
               .Without(r => r.Amenities)
               .Create();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);
        _reviewRepositoryMock.Setup(x => x.GetHotelAverageRatingAsync(hotel)).ReturnsAsync(expectedRating);

        // Act
        double result = await sut.GetHotelAverageRatingAsync(hotel.Id);

        // Assert
        Assert.Equal(roundedExpectedRating, result);
    }

    [Fact]
    public async Task GetHotelAverageRatingAsync_ShouldThrowNotFoundException_WhenHotelDoesNotExist()
    {
        // Arrange
        Guid hotelId = Guid.NewGuid();
        _hotelRepositoryMock.Setup(x => x.GetHotelAsync(hotelId)).ReturnsAsync((Hotel?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetHotelAverageRatingAsync(hotelId));
    }
}
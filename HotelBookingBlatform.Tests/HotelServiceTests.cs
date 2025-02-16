using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests;

public class HotelServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IImageHandler> _mockImageHandler;
    private readonly Mock<ILogger<HotelService>> _mockLogger;
    private readonly Mock<IEntityValidatorService> _entityValidatorServiceMock;
    private readonly HotelService sut;

    public HotelServiceTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _mockImageHandler = new Mock<IImageHandler>();
        _mockLogger = new Mock<ILogger<HotelService>>();
        _entityValidatorServiceMock = new Mock<IEntityValidatorService>();
        sut = new HotelService(
            _mockHotelRepository.Object,
            _mapper,
            _mockImageHandler.Object,
            _mockLogger.Object,
            _entityValidatorServiceMock.Object
        );
        _fixture = FixtureFactory.CreateFixture();
    }

    [Theory]
    [InlineData(1, 5, 5, false, false)]
    [InlineData(1, 10, 30, false, true)]
    [InlineData(2, 10, 30, true, true)]
    [InlineData(3, 10, 30, true, false)]
    public async Task GetAllHotelsAsync_ShouldHandlePaginationCorrectly(int pageNumber, int pageSize, int totalCount, bool HasPreviousPage, bool HasNextPage)
    {
        // Arrange
        var requestParameters = new GetHotelsQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        var expectedHotels = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .CreateMany(totalCount)
             .ToList();
        var expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        var skippedHotels = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        var expectedPaginatedResult = new PaginatedResult<Hotel>(expectedHotels.Skip(skippedHotels).Take(pageSize).ToList(), expectedPaginationMetadata);
        _mockHotelRepository.Setup(repo => repo.GetAllHotelsAsync(requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        var paginatedResult = await sut.GetAllHotelsAsync(requestParameters);

        // Assert
        _mockHotelRepository.Verify(c => c.GetAllHotelsAsync(requestParameters), Times.Once);
        Assert.Equal(pageSize, paginatedResult.Data.Count());
        Assert.Equal(pageNumber, paginatedResult.Metadata.PageNumber);
        Assert.Equal(pageSize, paginatedResult.Metadata.PageSize);
        Assert.Equal(totalCount, paginatedResult.Metadata.TotalCount);
        Assert.Equal(HasPreviousPage, paginatedResult.Metadata.HasPreviousPage);
        Assert.Equal(HasNextPage, paginatedResult.Metadata.HasNextPage);
    }

    [Fact]
    public async Task GetHotelAsync_ShouldReturnHotel_IfHotelExists()
    {
        // Arrange
        var hotel = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .Create();
        _entityValidatorServiceMock.Setup(x => x.ValidateHotelAsync(hotel.Id)).ReturnsAsync(hotel);

        // Act
        var result = await sut.GetHotelAsync(hotel.Id);

        // Assert
        _entityValidatorServiceMock.Verify(h => h.ValidateHotelAsync(hotel.Id), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<HotelWithFullDataOutputDto>(result);
        Assert.Equal(hotel.Name, result.Name);
    }

    [Fact]
    public async Task GetHotelAsync_ShouldThrowNotFoundException_IfHotelDoesNotExist()
    {
        // Arrange
        _entityValidatorServiceMock
            .Setup(x => x.ValidateHotelAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new NotFoundException(nameof(Hotel), It.IsAny<Guid>()));

        // Act & Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.GetHotelAsync(Guid.NewGuid()));
        _entityValidatorServiceMock.Verify(h => h.ValidateHotelAsync(It.IsAny<Guid>()), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteHotelAsync_ShouldCallSaveChangesAndReturnTrue_IfHotelExists()
    {
        // Arrange
        _mockHotelRepository.Setup(x => x.DeleteHotelAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await sut.DeleteHotelAsync(It.IsAny<Guid>());

        // Assert
        _mockHotelRepository.Verify(h => h.DeleteHotelAsync(It.IsAny<Guid>()), Times.Once);
        _mockHotelRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteHotelAsync_ShouldNotCallSaveChangesAndReturnFalse_IfHotelDoesNotExist()
    {
        // Arrange
        _mockHotelRepository.Setup(x => x.DeleteHotelAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await sut.DeleteHotelAsync(It.IsAny<Guid>());

        // Assert
        _mockHotelRepository.Verify(h => h.DeleteHotelAsync(It.IsAny<Guid>()), Times.Once);
        _mockHotelRepository.Verify(c => c.SaveChangesAsync(), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task CreateHotelAsync_ShouldReturnCreatedHotel_IfCityExists()
    {
        // Arrange
        var createHotelDto = _fixture.Create<CreateHotelDto>();
        var city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _entityValidatorServiceMock.Setup(x => x.ValidateCityAsync(createHotelDto.CityId)).ReturnsAsync(city);
        var hotel = _mapper.Map<Hotel>(createHotelDto);
        _mockHotelRepository.Setup(x => x.AddHotelAsync(It.IsAny<Hotel>())).ReturnsAsync(hotel);

        // Act
        var result = await sut.CreateHotelAsync(createHotelDto);

        // Assert
        _entityValidatorServiceMock.Verify(c => c.ValidateCityAsync(createHotelDto.CityId), Times.Once);
        _mockHotelRepository.Verify(h => h.AddHotelAsync(It.IsAny<Hotel>()), Times.Once);
        _mockHotelRepository.Verify(h => h.SaveChangesAsync(), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<HotelOutputDto>(result);
        Assert.Equal(createHotelDto.Name, result.Name);
    }

    [Fact]
    public async Task CreateHotelAsync_ShouldThrowNotFoundException_IfCityDoesNotExist()
    {
        // Arrange
        var createHotelDto = _fixture.Create<CreateHotelDto>();
        _entityValidatorServiceMock
            .Setup(x => x.ValidateCityAsync(createHotelDto.CityId))
            .ThrowsAsync(new NotFoundException(nameof(City), createHotelDto.CityId));

        // Act & Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateHotelAsync(createHotelDto));
        _entityValidatorServiceMock.Verify(c => c.ValidateCityAsync(createHotelDto.CityId), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateHotelAsync_ShouldCallSaveChangesAndReturnTrue_IfHotelAndCityExist()
    {
        // Arrange
        var updateHotelDto = _fixture.Create<UpdateHotelDto>();
        var hotel = _mapper.Map<Hotel>(updateHotelDto);
        var city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _entityValidatorServiceMock.Setup(x => x.ValidateCityAsync(updateHotelDto.CityId)).ReturnsAsync(city);
        _entityValidatorServiceMock.Setup(x => x.ValidateHotelAsync(It.IsAny<Guid>())).ReturnsAsync(hotel);

        // Act
        var result = await sut.UpdateHotelAsync(It.IsAny<Guid>(), updateHotelDto);

        // Assert
        _entityValidatorServiceMock.Verify(h => h.ValidateHotelAsync(It.IsAny<Guid>()), Times.Once);
        _mockHotelRepository.Verify(h => h.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateHotelAsync_ShouldThrowNotFoundException_IfCityDoesNotExist()
    {
        // Arrange
        var updateHotelDto = _fixture.Create<UpdateHotelDto>();
        _entityValidatorServiceMock
            .Setup(x => x.ValidateCityAsync(updateHotelDto.CityId))
            .ThrowsAsync(new NotFoundException(nameof(City), updateHotelDto.CityId));

        // Act & Asser
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateHotelAsync(It.IsAny<Guid>(), updateHotelDto));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateHotelAsync_ShouldThrowNotFoundException_IfHotelDoesNotExist()
    {
        // Arrange
        var updateHotelDto = _fixture.Create<UpdateHotelDto>();
        var city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _entityValidatorServiceMock.Setup(x => x.ValidateCityAsync(updateHotelDto.CityId)).ReturnsAsync(city);

        _entityValidatorServiceMock
            .Setup(x => x.ValidateHotelAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new NotFoundException(nameof(Hotel), It.IsAny<Guid>()));
        // Act & Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateHotelAsync(It.IsAny<Guid>(), updateHotelDto));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnTrue_WhenImageUploaded()
    {
        // Arrange
        var hotelId = Guid.NewGuid();
        var file = new Mock<IFormFile>();
        var basePath = "test/path";
        var alternativeText = "Test Image";
        var uploadedImageUrl = "http://test.com/image.jpg";
        var expectedImagePath = Path.Combine(basePath, "images", "hotels", hotelId.ToString());

        var hotel = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .Create();

        _entityValidatorServiceMock
            .Setup(x => x.ValidateHotelAsync(hotelId))
             .ReturnsAsync(hotel);

        _mockImageHandler
            .Setup(handler => handler.UploadImageAsync(file.Object, expectedImagePath, false))
            .ReturnsAsync(uploadedImageUrl);

        // Act
        var result = await sut.UploadImageAsync(hotelId, file.Object, basePath, alternativeText);

        // Assert
        _entityValidatorServiceMock.Verify(h => h.ValidateHotelAsync(hotelId), Times.Once);
        _mockImageHandler.Verify(h =>
            h.UploadImageAsync(file.Object, expectedImagePath, false),
            Times.Once
        );
        _mockHotelRepository.Verify(h =>
            h.AddHotelImageAsync(
                hotel,
                It.Is<HotelImage>(img =>
                    img.HotelId == hotel.Id &&
                    img.ImageUrl == uploadedImageUrl &&
                    img.AlternativeText == alternativeText
                )
            ),
            Times.Once
        );
        _mockHotelRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldThrowBadFileException_WhenBasePathIsInvalid()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var file = new Mock<IFormFile>();
        var basePath = "";
        var alternativeText = "Test Image";

        // Act & Assert
        await Assert.ThrowsAsync<BadFileException>(() => sut.UploadImageAsync(roomId, file.Object, basePath, alternativeText));
    }
}

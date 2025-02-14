using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
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
    private readonly Mock<ICityRepository> _mockCityRepository;
    private readonly Mock<IGuestRepository> _mockGuestRepository;
    private readonly Mock<IAmenityRepository> _amenityRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IImageHandler> _mockImageHandler;
    private readonly Mock<ILogger<HotelService>> _mockLogger;
    private readonly HotelService sut;

    public HotelServiceTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _mockCityRepository = new Mock<ICityRepository>();
        _mockGuestRepository = new Mock<IGuestRepository>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _mockImageHandler = new Mock<IImageHandler>();
        _mockLogger = new Mock<ILogger<HotelService>>();
        _amenityRepository = new Mock<IAmenityRepository>();
        sut = new HotelService(
            _mockHotelRepository.Object,
            _mockCityRepository.Object,
            _mockGuestRepository.Object,
            _amenityRepository.Object,
            _mapper,
            _mockImageHandler.Object,
            _mockLogger.Object
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
        GetHotelsQueryParametersDto requestParameters = new GetHotelsQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        List<Hotel> expectedHotels = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .CreateMany(totalCount)
             .ToList();
        PaginationMetadata expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        int skippedHotels = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        PaginatedResult<Hotel> expectedPaginatedResult = new PaginatedResult<Hotel>(expectedHotels.Skip(skippedHotels).Take(pageSize).ToList(), expectedPaginationMetadata);
        _mockHotelRepository.Setup(repo => repo.GetAllHotelsAsync(requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        PaginatedResult<HotelOutputDto> paginatedResult = await sut.GetAllHotelsAsync(requestParameters);

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
        Hotel hotel = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .Create();
        _mockHotelRepository.Setup(x => x.GetHotelAsync(hotel.Id)).ReturnsAsync(hotel);

        // Act
        HotelWithFullDataOutputDto? result = await sut.GetHotelAsync(hotel.Id);

        // Assert
        _mockHotelRepository.Verify(h => h.GetHotelAsync(hotel.Id), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<HotelWithFullDataOutputDto>(result);
        Assert.Equal(hotel.Name, result.Name);
    }

    [Fact]
    public async Task GetHotelAsync_ShouldThrowNotFoundException_IfHotelDoesNotExist()
    {
        // Arrange
        _mockHotelRepository.Setup(x => x.GetHotelAsync(It.IsAny<Guid>())).ReturnsAsync((Hotel?)null);

        // Act & Assert
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.GetHotelAsync(Guid.NewGuid()));
        _mockHotelRepository.Verify(h => h.GetHotelAsync(It.IsAny<Guid>()), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteHotelAsync_ShouldCallSaveChangesAndReturnTrue_IfHotelExists()
    {
        // Arrange
        _mockHotelRepository.Setup(x => x.DeleteHotelAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        bool result = await sut.DeleteHotelAsync(It.IsAny<Guid>());

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
        bool result = await sut.DeleteHotelAsync(It.IsAny<Guid>());

        // Assert
        _mockHotelRepository.Verify(h => h.DeleteHotelAsync(It.IsAny<Guid>()), Times.Once);
        _mockHotelRepository.Verify(c => c.SaveChangesAsync(), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task CreateHotelAsync_ShouldReturnCreatedHotel_IfCityExists()
    {
        // Arrange
        CreateHotelDto createHotelDto = _fixture.Create<CreateHotelDto>();
        City city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _mockCityRepository.Setup(x => x.GetCityAsync(createHotelDto.CityId)).ReturnsAsync(city);
        Hotel hotel = _mapper.Map<Hotel>(createHotelDto);
        _mockHotelRepository.Setup(x => x.AddHotelAsync(It.IsAny<Hotel>())).ReturnsAsync(hotel);

        // Act
        HotelOutputDto result = await sut.CreateHotelAsync(createHotelDto);

        // Assert
        _mockCityRepository.Verify(c => c.GetCityAsync(createHotelDto.CityId), Times.Once);
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
        CreateHotelDto createHotelDto = _fixture.Create<CreateHotelDto>();
        _mockCityRepository.Setup(x => x.GetCityAsync(createHotelDto.CityId)).ReturnsAsync((City?)null);

        // Act & Assert
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateHotelAsync(createHotelDto));
        _mockCityRepository.Verify(c => c.GetCityAsync(createHotelDto.CityId), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateHotelAsync_ShouldCallSaveChangesAndReturnTrue_IfHotelAndCityExist()
    {
        // Arrange
        UpdateHotelDto updateHotelDto = _fixture.Create<UpdateHotelDto>();
        Hotel hotel = _mapper.Map<Hotel>(updateHotelDto);
        _mockHotelRepository.Setup(x => x.GetHotelAsync(It.IsAny<Guid>())).ReturnsAsync(hotel);
        City city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _mockCityRepository.Setup(x => x.GetCityAsync(updateHotelDto.CityId)).ReturnsAsync(city);

        // Act
        bool result = await sut.UpdateHotelAsync(It.IsAny<Guid>(), updateHotelDto);

        // Assert
        _mockHotelRepository.Verify(h => h.GetHotelAsync(It.IsAny<Guid>()), Times.Once);
        _mockHotelRepository.Verify(h => h.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateHotelAsync_ShouldThrowNotFoundException_IfCityDoesNotExist()
    {
        // Arrange
        UpdateHotelDto updateHotelDto = _fixture.Create<UpdateHotelDto>();
        _mockCityRepository.Setup(x => x.GetCityAsync(updateHotelDto.CityId)).ReturnsAsync((City?)null);

        // Act & Asser
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateHotelAsync(It.IsAny<Guid>(), updateHotelDto));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateHotelAsync_ShouldThrowNotFoundException_IfHotelDoesNotExist()
    {
        // Arrange
        _mockHotelRepository.Setup(x => x.GetHotelAsync(It.IsAny<Guid>())).ReturnsAsync((Hotel?)null);
        UpdateHotelDto updateHotelDto = _fixture.Create<UpdateHotelDto>();
        City city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _mockCityRepository.Setup(x => x.GetCityAsync(updateHotelDto.CityId)).ReturnsAsync(city);

        // Act & Assert
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateHotelAsync(It.IsAny<Guid>(), updateHotelDto));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnTrue_WhenImageUploaded()
    {
        // Arrange
        Guid hotelId = Guid.NewGuid();
        Mock<IFormFile> file = new Mock<IFormFile>();
        string basePath = "test/path";
        string alternativeText = "Test Image";
        string uploadedImageUrl = "http://test.com/image.jpg";
        string expectedImagePath = Path.Combine(basePath, "images", "hotels", hotelId.ToString());

        Hotel hotel = _fixture.Build<Hotel>()
             .Without(r => r.Images)
             .Without(r => r.City)
             .Without(r => r.Location)
             .Without(r => r.Rooms)
             .Without(r => r.Reviews)
             .Without(r => r.Amenities)
             .Create();

        _mockHotelRepository.Setup(repo => repo.GetHotelAsync(hotelId))
            .ReturnsAsync(hotel);

        _mockImageHandler
            .Setup(handler => handler.UploadImageAsync(file.Object, expectedImagePath, false))
            .ReturnsAsync(uploadedImageUrl);

        // Act
        bool result = await sut.UploadImageAsync(hotelId, file.Object, basePath, alternativeText);

        // Assert
        _mockHotelRepository.Verify(h => h.GetHotelAsync(hotelId), Times.Once);
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
        Guid roomId = Guid.NewGuid();
        Mock<IFormFile> file = new Mock<IFormFile>();
        string basePath = "";
        string alternativeText = "Test Image";

        // Act & Assert
        await Assert.ThrowsAsync<BadFileException>(() => sut.UploadImageAsync(roomId, file.Object, basePath, alternativeText));
    }
}

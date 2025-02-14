using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace HotelBookingBlatform.Application.Tests;
public class CityServiceTests
{
    private readonly IFixture _fixture;
    private readonly Mock<ICityRepository> _mockCityRepository;
    private readonly IMapper _mapper;
    private readonly Mock<IImageHandler> _mockImageHandler;
    private readonly Mock<ILogger<CityService>> _mockLogger;
    private readonly CityService sut;
    public CityServiceTests()
    {
        _mockCityRepository = new Mock<ICityRepository>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _mockImageHandler = new Mock<IImageHandler>();
        _mockLogger = new Mock<ILogger<CityService>>();
        sut = new CityService(_mockCityRepository.Object, _mapper, _mockImageHandler.Object, _mockLogger.Object);
        _fixture = FixtureFactory.CreateFixture();

    }

    [Theory]
    [InlineData(1, 5, 5, false, false)]
    [InlineData(1, 10, 30, false, true)]
    [InlineData(2, 10, 30, true, true)]
    [InlineData(3, 10, 30, true, false)]
    public async Task GetAllCitiesAsync_ShouldHandlePaginationCorrectly(int pageNumber, int pageSize, int totalCount, bool HasPreviousPage, bool HasNextPage)
    {
        // Arrange
        GetCitiesQueryParametersDto requestParameters = new GetCitiesQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        List<City> expectedCities = _fixture.Build<City>()
            .Without(c => c.Hotels) // Avoid recursion by skipping navigation properties
            .Without(c => c.Images)
            .CreateMany(totalCount)
            .ToList();
        PaginationMetadata expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        int skippedCities = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        PaginatedResult<City> expectedPaginatedResult = new PaginatedResult<City>(expectedCities.Skip(skippedCities).Take(pageSize).ToList(), expectedPaginationMetadata);
        _mockCityRepository.Setup(repo => repo.GetAllCitiesAsync(requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        PaginatedResult<CityOutputDto> paginatedResult = await sut.GetAllCitiesAsync(requestParameters);

        // Assert
        _mockCityRepository.Verify(c => c.GetAllCitiesAsync(requestParameters), Times.Once);
        Assert.Equal(pageSize, paginatedResult.Data.Count());
        Assert.Equal(pageNumber, paginatedResult.Metadata.PageNumber);
        Assert.Equal(pageSize, paginatedResult.Metadata.PageSize);
        Assert.Equal(totalCount, paginatedResult.Metadata.TotalCount);
        Assert.Equal(HasPreviousPage, paginatedResult.Metadata.HasPreviousPage);
        Assert.Equal(HasNextPage, paginatedResult.Metadata.HasNextPage);
    }

    [Fact]
    public async Task GetCityAsync_ShouldReturnCityOutputWithHotelsAndImagesDto()
    {
        // Arrange
        City city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();
        _mockCityRepository.Setup(x => x.GetCityAsync(city.Id)).ReturnsAsync(city);

        // Act
        CityOutputWithHotelsAndImagesDto? result = await sut.GetCityAsync(city.Id);

        // Assert
        _mockCityRepository.Verify(c => c.GetCityAsync(city.Id), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<CityOutputWithHotelsAndImagesDto>(result);
        Assert.Equal(city.Name, result.Name);
    }

    [Fact]
    public async Task GetCityAsync_ShouldThrowNotFoundException_IfCityDoesNotExist()
    {
        // Arrange
        _mockCityRepository.Setup(x => x.GetCityAsync(It.IsAny<Guid>())).ReturnsAsync((City?)null);

        // Act & Assert
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.GetCityAsync(It.IsAny<Guid>()));
        _mockCityRepository.Verify(c => c.GetCityAsync(It.IsAny<Guid>()), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task DeleteCityAsync_ShouldCallSaveChangesAsyncAndReturnTrue_IfCityExists()
    {
        // Arrange
        _mockCityRepository.Setup(x => x.DeleteCityAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        bool result = await sut.DeleteCityAsync(It.IsAny<Guid>());

        // Assert
        _mockCityRepository.Verify(c => c.DeleteCityAsync(It.IsAny<Guid>()), Times.Once);
        _mockCityRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteCityAsync_ShouldNotCallSaveChangesAsyncAndReturnFalse_IfCityDoesNotExist()
    {
        // Arrange
        _mockCityRepository.Setup(x => x.DeleteCityAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        bool result = await sut.DeleteCityAsync(It.IsAny<Guid>());

        // Assert
        _mockCityRepository.Verify(c => c.DeleteCityAsync(It.IsAny<Guid>()), Times.Once);
        _mockCityRepository.Verify(c => c.SaveChangesAsync(), Times.Never);
        Assert.False(result);

    }

    [Fact]
    public async Task CreateCityAsync_ShouldReturnCreatedCity()
    {
        // Arrange
        CreateCityDto createdCity = _fixture.Create<CreateCityDto>();
        City city = _mapper.Map<City>(createdCity);
        _mockCityRepository.Setup(x => x.AddCityAsync(It.IsAny<City>())).ReturnsAsync(city);

        // Act
        CityOutputDto result = await sut.CreateCityAsync(createdCity);

        // Assert
        _mockCityRepository.Verify(c => c.AddCityAsync(It.IsAny<City>()), Times.Once);
        _mockCityRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.Equal(city.Name, result.Name);
    }

    [Fact]
    public async Task UpdateCityAsync_ShouldCallSaveChangesAndReturnTrue_IfCityExists()
    {
        // Arrange
        UpdateCityDto updateCityCommand = _fixture.Create<UpdateCityDto>();
        City city = _mapper.Map<City>(updateCityCommand);
        _mockCityRepository.Setup(x => x.GetCityAsync(It.IsAny<Guid>())).ReturnsAsync(city);

        // Act
        bool result = await sut.UpdateCityAsync(It.IsAny<Guid>(), updateCityCommand);

        // Assert
        _mockCityRepository.Verify(c => c.GetCityAsync(It.IsAny<Guid>()), Times.Once);
        _mockCityRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }
    [Fact]
    public async Task UpdateCityAsync_ShouldThrowNotFoundException_IfCityDoesNotExist()
    {
        // Arrange
        _mockCityRepository.Setup(x => x.GetCityAsync(It.IsAny<Guid>())).ReturnsAsync((City?)null);

        // Act & Asser
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateCityAsync(It.IsAny<Guid>(), It.IsAny<UpdateCityDto>()));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task MostVisitedCitiesAsync_ShouldReturnCities()
    {
        // Arrange
        int count = 2;
        List<City> cities = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .CreateMany(2)
            .ToList();

        List<CityAsTrendingDestinationOutputDto> mappedCities = _mapper.Map<List<CityAsTrendingDestinationOutputDto>>(cities);
        _mockCityRepository.Setup(repo => repo.GetMostVisitedCitiesAsync(count)).ReturnsAsync(cities);

        // Act
        IEnumerable<CityAsTrendingDestinationOutputDto> result = await sut.MostVisitedCitiesAsync(count);

        // Assert
        _mockCityRepository.Verify(c => c.GetMostVisitedCitiesAsync(count), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<List<CityAsTrendingDestinationOutputDto>>(result);
        Assert.Equal(2, result.Count());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task MostVisitedCitiesAsync_ShouldThrowBadRequestException_WhenCountIsInvalid(int invalidCount)
    {
        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => sut.MostVisitedCitiesAsync(invalidCount));
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnTrue_WhenImageUploaded()
    {
        // Arrange
        Guid cityId = Guid.NewGuid();
        Mock<IFormFile> file = new Mock<IFormFile>();
        string basePath = "test/path";
        string alternativeText = "Test Image";
        string uploadedImageUrl = "http://test.com/image.jpg";
        string expectedImagePath = Path.Combine(basePath, "images", "cities", cityId.ToString());

        City city = _fixture.Build<City>()
            .Without(c => c.Hotels)
            .Without(c => c.Images)
            .Create();

        _mockCityRepository.Setup(repo => repo.GetCityAsync(cityId))
            .ReturnsAsync(city);

        _mockImageHandler
            .Setup(handler => handler.UploadImageAsync(file.Object, expectedImagePath, false))
            .ReturnsAsync(uploadedImageUrl);

        // Act
        bool result = await sut.UploadImageAsync(cityId, file.Object, basePath, alternativeText);

        // Assert
        _mockCityRepository.Verify(c => c.GetCityAsync(cityId), Times.Once);
        _mockImageHandler.Verify(c =>
            c.UploadImageAsync(file.Object, expectedImagePath, false),
            Times.Once
        );
        _mockCityRepository.Verify(c =>
            c.AddCityImageAsync(
                city,
                It.Is<CityImage>(img =>
                    img.CityId == city.Id &&
                    img.ImageUrl == uploadedImageUrl &&
                    img.AlternativeText == alternativeText
                )
            ),
            Times.Once
        );
        _mockCityRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldThrowBadFileException_WhenBasePathIsInvalid()
    {
        // Arrange
        Guid cityId = Guid.NewGuid();
        Mock<IFormFile> file = new Mock<IFormFile>();
        string basePath = "";
        string alternativeText = "Test Image";

        // Act & Assert
        await Assert.ThrowsAsync<BadFileException>(() => sut.UploadImageAsync(cityId, file.Object, basePath, alternativeText));
    }

    [Fact]
    public async Task CityExistsAsync_ShouldReturnTrue_WhenCityExists()
    {
        // Arrange
        _mockCityRepository.Setup(repo => repo.CityExistsAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        bool result = await sut.CityExistsAsync(It.IsAny<Guid>());

        // Assert
        _mockCityRepository.Verify(c => c.CityExistsAsync(It.IsAny<Guid>()), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task CityExistsAsync_ShouldReturnFalse_WhenCityDoesNotExist()
    {
        // Arrang
        _mockCityRepository.Setup(repo => repo.CityExistsAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        bool result = await sut.CityExistsAsync(It.IsAny<Guid>());

        // Assert
        _mockCityRepository.Verify(c => c.CityExistsAsync(It.IsAny<Guid>()), Times.Once);
        Assert.False(result);
    }
}
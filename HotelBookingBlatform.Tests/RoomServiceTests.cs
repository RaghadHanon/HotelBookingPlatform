using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace HotelBookingBlatform.Application.Tests;
public class RoomServiceTests
{
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IImageHandler> _mockImageHandler;
    private readonly RoomService sut;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    private readonly Mock<IEntityValidatorService> _entityValidatorServiceMock;

    public RoomServiceTests()
    {
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockImageHandler = new Mock<IImageHandler>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        _entityValidatorServiceMock = new Mock<IEntityValidatorService>();
        sut = new RoomService(
           _mockRoomRepository.Object,
           _mapper,
           _mockImageHandler.Object,
           _entityValidatorServiceMock.Object
        );
        _fixture = FixtureFactory.CreateFixture();
    }

    [Theory]
    [InlineData(1, 5, 5, false, false)]
    [InlineData(1, 10, 30, false, true)]
    [InlineData(2, 10, 30, true, true)]
    [InlineData(3, 10, 30, true, false)]
    public async Task GetAllRoomsAsync_ShouldHandlePaginationCorrectly(int pageNumber, int pageSize, int totalCount, bool HasPreviousPage, bool HasNextPage)
    {
        // Arrange
        var requestParameters = new GetRoomsQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        var expectedRooms = _fixture.Build<Room>()
            .Without(r => r.Images)
            .Without(r => r.Discounts)
            .Without(r => r.Hotel)
            .Without(r => r.BookingRooms)
            .Without(r => r.Amenities)
            .CreateMany(totalCount)
            .ToList();
        var expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        var skippedRooms = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        var expectedPaginatedResult = new PaginatedResult<Room>(expectedRooms.Skip(skippedRooms).Take(pageSize).ToList(), expectedPaginationMetadata);
        _mockRoomRepository.Setup(repo => repo.GetAllRoomsAsync(requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        var paginatedResult = await sut.GetAllRoomsAsync(requestParameters);

        // Assert
        _mockRoomRepository.Verify(c => c.GetAllRoomsAsync(requestParameters), Times.Once);
        Assert.Equal(pageSize, paginatedResult.Data.Count());
        Assert.Equal(pageNumber, paginatedResult.Metadata.PageNumber);
        Assert.Equal(pageSize, paginatedResult.Metadata.PageSize);
        Assert.Equal(totalCount, paginatedResult.Metadata.TotalCount);
        Assert.Equal(HasPreviousPage, paginatedResult.Metadata.HasPreviousPage);
        Assert.Equal(HasNextPage, paginatedResult.Metadata.HasNextPage);
    }

    [Fact]
    public async Task GetRoomAsync_ShouldReturnRoomOutputWithImagesDto_IfRoomExists()
    {
        // Arrange
        var room = _fixture.Build<Room>()
           .Without(r => r.Images)
           .Without(r => r.Discounts)
           .Without(r => r.Hotel)
           .Without(r => r.BookingRooms)
           .Without(r => r.Amenities)
           .Create();
        _entityValidatorServiceMock.Setup(x => x.ValidateRoomAsync(room.Id)).ReturnsAsync(room);

        // Act
        var result = await sut.GetRoomAsync(room.Id);

        // Assert
        _entityValidatorServiceMock.Verify(r => r.ValidateRoomAsync(room.Id), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<RoomWithFullDataOutputDto>(result);
        Assert.Equal(room.RoomNumber, result.RoomNumber);
    }

    [Fact]
    public async Task GetRoomAsync_ShouldThrowNotFoundException_IfRoomDoesNotExist()
    {
        // Arrange
        _entityValidatorServiceMock
            .Setup(x => x.ValidateRoomAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new NotFoundException(nameof(Room), It.IsAny<Guid>()));

        // Act & Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.GetRoomAsync(It.IsAny<Guid>()));
        _entityValidatorServiceMock.Verify(r => r.ValidateRoomAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoomAsync_ShouldCallSaveChangesAndReturnTrue_IfRoomExists()
    {
        // Arrange
        _mockRoomRepository.Setup(x => x.DeleteRoomAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await sut.DeleteRoomAsync(It.IsAny<Guid>());

        // Assert
        _mockRoomRepository.Verify(c => c.DeleteRoomAsync(It.IsAny<Guid>()), Times.Once);
        _mockRoomRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteRoomAsync_ShouldNotCallSaveChangesAndReturnFalse_IfRoomDoesNotExist()
    {
        // Arrange
        _mockRoomRepository.Setup(x => x.DeleteRoomAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await sut.DeleteRoomAsync(It.IsAny<Guid>());

        // Assert
        _mockRoomRepository.Verify(r => r.DeleteRoomAsync(It.IsAny<Guid>()), Times.Once);
        _mockRoomRepository.Verify(c => c.SaveChangesAsync(), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task CreateRoomAsync_ShouldReturnCreatedRoom_IfHotelNameExists()
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
        var room = _fixture.Build<Room>()
           .Without(r => r.Images)
           .Without(r => r.Discounts)
           .Without(r => r.Hotel)
           .Without(r => r.BookingRooms)
           .Without(r => r.Amenities)
           .Create();
        var createRoomDto = _fixture.Build<CreateRoomDto>()
            .With(x => x.HotelId, hotel.Id)
            .Create();

        _entityValidatorServiceMock.Setup(x => x.ValidateHotelAsync(createRoomDto.HotelId)).ReturnsAsync(hotel);
        _mockRoomRepository.Setup(x => x.AddRoomAsync(It.IsAny<Room>())).ReturnsAsync(room);

        // Act
        var result = await sut.CreateRoomAsync(createRoomDto);

        // Assert
        _entityValidatorServiceMock.Verify(h => h.ValidateHotelAsync(createRoomDto.HotelId), Times.Once);
        _mockRoomRepository.Verify(r => r.AddRoomAsync(It.IsAny<Room>()), Times.Once);
        _mockRoomRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<RoomOutputDto>(result);
        Assert.Equal(room.RoomNumber, result.RoomNumber);
    }

    [Fact]
    public async Task CreateRoomAsync_ShouldThrowNotFoundException_IfHotelNameDoesNotExist()
    {
        // Arrange
        var createRoomDto = _fixture.Build<CreateRoomDto>()
           .Without(r => r.HotelId)
           .Create();
        _entityValidatorServiceMock
            .Setup(x => x.ValidateHotelAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new NotFoundException(nameof(Hotel), createRoomDto.HotelId));

        // Act & Assert
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateRoomAsync(createRoomDto));
        _entityValidatorServiceMock.Verify(h => h.ValidateHotelAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomAsync_ShouldCallSaveChangesAndReturnTrue_IfRoomExists()
    {
        // Arrange
        var updateRoomDto = _fixture.Create<UpdateRoomDto>();
        var room = _mapper.Map<Room>(updateRoomDto);
        _entityValidatorServiceMock.Setup(x => x.ValidateRoomAsync(It.IsAny<Guid>())).ReturnsAsync(room);

        // Act
        var result = await sut.UpdateRoomAsync(It.IsAny<Guid>(), updateRoomDto);

        // Assert
        _entityValidatorServiceMock.Verify(c => c.ValidateRoomAsync(It.IsAny<Guid>()), Times.Once);
        _mockRoomRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateRoomAsync_ShouldThrowNotFoundException_IfRoomDoesNotExist()
    {
        // Arrange
        _entityValidatorServiceMock
            .Setup(x => x.ValidateRoomAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new NotFoundException(nameof(Room), It.IsAny<Guid>()));

        // Act & Asser
        var result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateRoomAsync(It.IsAny<Guid>(), It.IsAny<UpdateRoomDto>()));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnTrue_WhenImageUploaded()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var file = new Mock<IFormFile>();
        var basePath = "test/path";
        var alternativeText = "Test Image";
        var uploadedImageUrl = "http://test.com/image.jpg";
        var expectedImagePath = Path.Combine(basePath, "images", "rooms", roomId.ToString());

        var room = _fixture.Build<Room>()
           .Without(r => r.Images)
           .Without(r => r.Discounts)
           .Without(r => r.Hotel)
           .Without(r => r.BookingRooms)
           .Without(r => r.Amenities)
           .Create();

        _entityValidatorServiceMock.Setup(repo => repo.ValidateRoomAsync(roomId))
            .ReturnsAsync(room);

        _mockImageHandler
            .Setup(handler => handler.UploadImageAsync(file.Object, expectedImagePath, false))
            .ReturnsAsync(uploadedImageUrl);

        // Act
        var result = await sut.UploadImageAsync(roomId, file.Object, basePath, alternativeText);

        // Assert
        _entityValidatorServiceMock.Verify(r => r.ValidateRoomAsync(roomId), Times.Once);
        _mockImageHandler.Verify(r =>
            r.UploadImageAsync(file.Object, expectedImagePath, false),
            Times.Once
        );
        _mockRoomRepository.Verify(r =>
            r.AddRoomImageAsync(
                room,
                It.Is<RoomImage>(img =>
                    img.RoomId == room.Id &&
                    img.ImageUrl == uploadedImageUrl &&
                    img.AlternativeText == alternativeText
                )
            ),
            Times.Once
        );
        _mockRoomRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
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
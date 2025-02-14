using AutoFixture;
using AutoMapper;
using HotelBookingBlatform.Application.Tests.Common;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Moq;

namespace HotelBookingBlatform.Application.Tests;
public class RoomServiceTests
{
    private readonly Mock<IHotelRepository> _mockHotelRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IImageHandler> _mockImageHandler;
    private readonly Mock<IAmenityRepository> _mockAmenityRepository;
    private readonly RoomService sut;
    private readonly IFixture _fixture;
    private readonly IMapper _mapper;
    public RoomServiceTests()
    {
        _mockHotelRepository = new Mock<IHotelRepository>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockAmenityRepository = new Mock<IAmenityRepository>();
        _mockImageHandler = new Mock<IImageHandler>();
        _mapper = AutoMapperSingleton.GetMapperAsync().Result;
        sut = new RoomService(
           _mockHotelRepository.Object,
           _mockRoomRepository.Object,
           _mockAmenityRepository.Object,
           _mapper,
           _mockImageHandler.Object
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
        GetRoomsQueryParametersDto requestParameters = new GetRoomsQueryParametersDto { PageNumber = pageNumber, PageSize = pageSize };
        List<Room> expectedRooms = _fixture.Build<Room>()
            .Without(r => r.Images)
            .Without(r => r.Discounts)
            .Without(r => r.Hotel)
            .Without(r => r.BookingRooms)
            .Without(r => r.Amenities)
            .CreateMany(totalCount)
            .ToList();
        PaginationMetadata expectedPaginationMetadata = new PaginationMetadata(pageNumber, pageSize, totalCount);
        int skippedRooms = pageNumber > 1 ? (pageNumber - 1) * pageSize : 0;
        PaginatedResult<Room> expectedPaginatedResult = new PaginatedResult<Room>(expectedRooms.Skip(skippedRooms).Take(pageSize).ToList(), expectedPaginationMetadata);
        _mockRoomRepository.Setup(repo => repo.GetAllRoomsAsync(requestParameters)).ReturnsAsync(expectedPaginatedResult);

        // Act
        PaginatedResult<RoomOutputDto> paginatedResult = await sut.GetAllRoomsAsync(requestParameters);

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
        Room room = _fixture.Build<Room>()
           .Without(r => r.Images)
           .Without(r => r.Discounts)
           .Without(r => r.Hotel)
           .Without(r => r.BookingRooms)
           .Without(r => r.Amenities)
           .Create();
        _mockRoomRepository.Setup(x => x.GetRoomAsync(room.Id)).ReturnsAsync(room);

        // Act
        RoomWithFullDataOutputDto? result = await sut.GetRoomAsync(room.Id);

        // Assert
        _mockRoomRepository.Verify(r => r.GetRoomAsync(room.Id), Times.Once);
        Assert.NotNull(result);
        Assert.IsType<RoomWithFullDataOutputDto>(result);
        Assert.Equal(room.RoomNumber, result.RoomNumber);
    }

    [Fact]
    public async Task GetRoomAsync_ShouldThrowNotFoundException_IfRoomDoesNotExist()
    {
        // Arrange
        _mockRoomRepository.Setup(x => x.GetRoomAsync(It.IsAny<Guid>())).ReturnsAsync((Room?)null);

        // Act & Assert
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.GetRoomAsync(It.IsAny<Guid>()));
        _mockRoomRepository.Verify(r => r.GetRoomAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoomAsync_ShouldCallSaveChangesAndReturnTrue_IfRoomExists()
    {
        // Arrange
        _mockRoomRepository.Setup(x => x.DeleteRoomAsync(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        bool result = await sut.DeleteRoomAsync(It.IsAny<Guid>());

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
        bool result = await sut.DeleteRoomAsync(It.IsAny<Guid>());

        // Assert
        _mockRoomRepository.Verify(r => r.DeleteRoomAsync(It.IsAny<Guid>()), Times.Once);
        _mockRoomRepository.Verify(c => c.SaveChangesAsync(), Times.Never);
        Assert.False(result);
    }

    [Fact]
    public async Task CreateRoomAsync_ShouldReturnCreatedRoom_IfHotelNameExists()
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
           .Without(r => r.Discounts)
           .Without(r => r.Hotel)
           .Without(r => r.BookingRooms)
           .Without(r => r.Amenities)
           .Create();
        CreateRoomDto createRoomDto = _fixture.Build<CreateRoomDto>()
            .With(x => x.HotelId, hotel.Id)
            .Create();

        _mockHotelRepository.Setup(x => x.GetHotelAsync(createRoomDto.HotelId)).ReturnsAsync(hotel);
        _mockRoomRepository.Setup(x => x.AddRoomAsync(It.IsAny<Room>())).ReturnsAsync(room);

        // Act
        RoomOutputDto result = await sut.CreateRoomAsync(createRoomDto);

        // Assert
        _mockHotelRepository.Verify(h => h.GetHotelAsync(createRoomDto.HotelId), Times.Once);
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
        CreateRoomDto createRoomDto = _fixture.Build<CreateRoomDto>()
           .Without(r => r.HotelId)
           .Create();
        _mockHotelRepository.Setup(x => x.GetHotelAsync(It.IsAny<Guid>())).ReturnsAsync((Hotel?)null);

        // Act & Assert
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.CreateRoomAsync(createRoomDto));
        _mockHotelRepository.Verify(h => h.GetHotelAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoomAsync_ShouldCallSaveChangesAndReturnTrue_IfRoomExists()
    {
        // Arrange
        UpdateRoomDto updateRoomDto = _fixture.Create<UpdateRoomDto>();
        Room room = _mapper.Map<Room>(updateRoomDto);
        _mockRoomRepository.Setup(x => x.GetRoomAsync(It.IsAny<Guid>())).ReturnsAsync(room);

        // Act
        bool result = await sut.UpdateRoomAsync(It.IsAny<Guid>(), updateRoomDto);

        // Assert
        _mockRoomRepository.Verify(c => c.GetRoomAsync(It.IsAny<Guid>()), Times.Once);
        _mockRoomRepository.Verify(c => c.SaveChangesAsync(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task UpdateRoomAsync_ShouldThrowNotFoundException_IfRoomDoesNotExist()
    {
        // Arrange
        _mockRoomRepository.Setup(x => x.GetRoomAsync(It.IsAny<Guid>())).ReturnsAsync((Room?)null);

        // Act & Asser
        NotFoundException result = await Assert.ThrowsAsync<NotFoundException>(() => sut.UpdateRoomAsync(It.IsAny<Guid>(), It.IsAny<UpdateRoomDto>()));
        Assert.NotNull(result);
    }

    [Fact]
    public async Task UploadImageAsync_ShouldReturnTrue_WhenImageUploaded()
    {
        // Arrange
        Guid roomId = Guid.NewGuid();
        Mock<IFormFile> file = new Mock<IFormFile>();
        string basePath = "test/path";
        string alternativeText = "Test Image";
        string uploadedImageUrl = "http://test.com/image.jpg";
        string expectedImagePath = Path.Combine(basePath, "images", "rooms", roomId.ToString());

        Room room = _fixture.Build<Room>()
           .Without(r => r.Images)
           .Without(r => r.Discounts)
           .Without(r => r.Hotel)
           .Without(r => r.BookingRooms)
           .Without(r => r.Amenities)
           .Create();

        _mockRoomRepository.Setup(repo => repo.GetRoomAsync(roomId))
            .ReturnsAsync(room);

        _mockImageHandler
            .Setup(handler => handler.UploadImageAsync(file.Object, expectedImagePath, false))
            .ReturnsAsync(uploadedImageUrl);

        // Act
        bool result = await sut.UploadImageAsync(roomId, file.Object, basePath, alternativeText);

        // Assert
        _mockRoomRepository.Verify(r => r.GetRoomAsync(roomId), Times.Once);
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
        Guid roomId = Guid.NewGuid();
        Mock<IFormFile> file = new Mock<IFormFile>();
        string basePath = "";
        string alternativeText = "Test Image";

        // Act & Assert
        await Assert.ThrowsAsync<BadFileException>(() => sut.UploadImageAsync(roomId, file.Object, basePath, alternativeText));
    }
}
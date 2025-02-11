using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;

namespace HotelBookingPlatform.Application.Services;

public class RoomService : IRoomService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;
    private readonly IImageHandler _imageHandler;

    public RoomService(IHotelRepository hotelRepository, IRoomRepository roomRepository,
        IAmenityRepository amenityRepository, IMapper mapper, IImageHandler imageHandler)
    {
        _hotelRepository = hotelRepository;
        _roomRepository = roomRepository;
        _mapper = mapper;
        _imageHandler = imageHandler;
        _amenityRepository = amenityRepository;
    }

    public async Task<PaginatedResult<RoomOutputDto>> GetAllRoomsAsync(GetRoomsQueryParametersDto parameters)
    {
        var paginatedResult = await _roomRepository.GetAllRoomsAsync(parameters);
        var mapped = _mapper.Map<IEnumerable<RoomOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<RoomOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<RoomWithFullDataOutputDto?> GetRoomAsync(Guid id)
    {
        var room = await _roomRepository.GetRoomAsync(id) ?? throw new NotFoundException(nameof(Room), id);
        var mapped = _mapper.Map<RoomWithFullDataOutputDto>(room);

        return mapped;
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        var deleted = await _roomRepository.DeleteRoomAsync(id);
        if (deleted)
        {
            await _roomRepository.SaveChangesAsync();
        }

        return deleted;
    }

    public async Task<RoomOutputDto> CreateRoomAsync(CreateRoomDto request)
    {
        var hotel = await _hotelRepository.GetHotelAsync(request.HotelId)
            ?? throw new NotFoundException(nameof(Hotel), request.HotelId);
        var room = _mapper.Map<Room>(request);
        room.Id = Guid.NewGuid();
        room.CreationDate = DateTime.UtcNow;
        room.LastModified = DateTime.UtcNow;
        room.Hotel = hotel;
        var createdRoom = await _roomRepository.AddRoomAsync(room);
        await _roomRepository.SaveChangesAsync();

        return _mapper.Map<RoomOutputDto>(createdRoom);
    }

    public async Task<bool> UpdateRoomAsync(Guid id, UpdateRoomDto request)
    {
        var room = await _roomRepository.GetRoomAsync(id) 
            ?? throw new NotFoundException(nameof(Room), id);
        _mapper.Map(request, room);
        room.LastModified = DateTime.UtcNow;
        await _roomRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UploadImageAsync(Guid roomId, IFormFile file, string basePath, string? alternativeText, bool? thumbnail = false)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new BadFileException(ServicesErrorMessages.ErrorWithUploadingImage);
        }

        var room = await _roomRepository.GetRoomAsync(roomId) ?? throw new NotFoundException(nameof(Room), roomId);
        var roomDirectory = Path.Combine(basePath, "images", "rooms", roomId.ToString());
        var uploadedImageUrl = await _imageHandler.UploadImageAsync(file, roomDirectory, thumbnail.GetValueOrDefault(false));
        var image = new RoomImage
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            ImageUrl = uploadedImageUrl,
            AlternativeText = alternativeText,
            RoomId = room.Id
        };
        room.LastModified = DateTime.UtcNow;
        await _roomRepository.AddRoomImageAsync(room, image);
        await _roomRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddAmenityToRoomAsync(Guid roomId, Guid anenityId)
    {
        var room = await _roomRepository.GetRoomAsync(roomId)
            ?? throw new NotFoundException(nameof(Room), roomId);
        var amenity = await _amenityRepository.GetAmenityAsync(anenityId)
            ?? throw new NotFoundException(nameof(Amenity), anenityId);
        room.LastModified = DateTime.UtcNow;
        _roomRepository.AddAmenityToRoomAsync(room, amenity);
        await _hotelRepository.SaveChangesAsync();

        return true;
    }

    public async Task<PaginatedResult<AmenityOutputDto>> GetAmenitiesForHotelAsync(Guid roomid, GetAmenitiesQueryParametersDto request)
    {
        var paginatedResult = await _roomRepository.GetAmenitiesForRoomAsync(roomid, request);
        var mapped = _mapper.Map<IEnumerable<AmenityOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<AmenityOutputDto>(mapped, paginatedResult.Metadata);
    }
}



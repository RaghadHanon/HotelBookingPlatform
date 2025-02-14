using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Room;
using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IRoomService
{
    Task<RoomOutputDto> CreateRoomAsync(CreateRoomDto request);
    Task<bool> DeleteRoomAsync(Guid id);
    Task<PaginatedResult<RoomOutputDto>> GetAllRoomsAsync(GetRoomsQueryParametersDto parameters);
    Task<RoomWithFullDataOutputDto?> GetRoomAsync(Guid id);
    Task<bool> AddAmenityToRoomAsync(Guid roomId, Guid anenityId);
    Task<PaginatedResult<AmenityOutputDto>> GetAmenitiesForHotelAsync(Guid roomid, GetAmenitiesQueryParametersDto request);
    Task<bool> UpdateRoomAsync(Guid id, UpdateRoomDto request);
    Task<bool> UploadImageAsync(Guid roomId, IFormFile file, string basePath, string? alternativeText, bool? thumbnail = false);
}

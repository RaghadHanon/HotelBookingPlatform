using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;

public interface IRoomRepository
{
    Task<Room> AddRoomAsync(Room room);
    Task<RoomImage> AddRoomImageAsync(Room room, RoomImage roomImage);
    Task<bool> RoomContainsAmenityAsync(Guid roomId, Guid amenityId);
    Task<bool> DeleteRoomAsync(Guid id);
    Task<PaginatedResult<Room>> GetAllRoomsAsync(GetRoomsQueryParametersDto request);
    Amenity AddAmenityToRoom(Room room, Amenity amenity);
    Task<PaginatedResult<Amenity>> GetAmenitiesForRoomAsync(Guid id, GetAmenitiesQueryParametersDto request);
    Task<Room?> GetRoomAsync(Guid id);
    Task<Discount?> GetActiveDiscountForRoom(Guid roomId, DateTime checkInDate, DateTime checkOutDate);
    Task<IEnumerable<Room>> GetRoomsWithMostRecentHighestDiscounts(int rooms);
    Task<bool> IsAvailableAsync(Guid roomId, DateOnly startDate, DateOnly endDate);
    Task<bool> SaveChangesAsync();
}

using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Room;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public RoomRepository(HotelBookingPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Room> AddRoomAsync(Room room)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Room> entry = await _dbContext.Rooms.AddAsync(room);

        return entry.Entity;
    }

    public async Task<RoomImage> AddRoomImageAsync(Room room, RoomImage roomImage)
    {
        await _dbContext.RoomImages.AddAsync(roomImage);
        room.Images.Add(roomImage);

        return roomImage;
    }

    public Amenity AddAmenityToRoomAsync(Room room, Amenity amenity)
    {
        amenity.Rooms.Add(room);
        room.Amenities.Add(amenity);

        return amenity;
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        Room? room = await _dbContext.Rooms.FindAsync(id);
        if (room is null)
        {
            return false;
        }

        _dbContext.Rooms.Remove(room);

        return true;
    }

    public async Task<Room?> GetRoomAsync(Guid id)
    {
        return await _dbContext.Rooms
            .Include(r => r.Hotel)
            .Include(r => r.Images)
            .Include(r => r.Amenities)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }

    public async Task<PaginatedResult<Amenity>> GetAmenitiesForRoomAsync(Guid id, GetAmenitiesQueryParametersDto request)
    {
        IQueryable<Amenity> query = _dbContext.Rooms
            .Where(r => r.Id == id)
            .Include(r => r.Amenities)
            .SelectMany(r => r.Amenities)
            .AsQueryable();

        query = SearchAmenitiesByNameOrDescription(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<Amenity> result = await query
            .AsNoTracking()
            .ToListAsync();

        return new PaginatedResult<Amenity>(result, paginationMetadata);
    }

    private IQueryable<Amenity> SearchAmenitiesByNameOrDescription(IQueryable<Amenity> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(a => a.Name.StartsWith(searchTerm) || (!string.IsNullOrWhiteSpace(a.Description) && a.Description!.StartsWith(searchTerm)));
        }

        return query;
    }


    public async Task<bool> IsAvailableAsync(Guid roomId, DateOnly startDate, DateOnly endDate)
    {
        return await _dbContext.Rooms
            .Where(r => r.Id == roomId)
            .AllAsync(r => r.BookingRooms.All(b => startDate >= b.Booking.CheckOutDate || endDate <= b.Booking.CheckInDate));
    }

    public async Task<PaginatedResult<Room>> GetAllRoomsAsync(GetRoomsQueryParametersDto request)
    {
        IQueryable<Room> query = _dbContext.Rooms
           .Include(r => r.Hotel)
           .AsQueryable();
        query = SearchInHotelName(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<Room> result = await query
            .AsNoTracking()
            .ToListAsync();

        return new PaginatedResult<Room>(result, paginationMetadata);
    }

    private static IQueryable<Room> SearchInHotelName(IQueryable<Room> query, string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(r => r.Hotel.Name.Contains(searchTerm));
        }

        return query;
    }

    public async Task<Discount?> GetActiveDiscountForRoom(Guid roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        return await _dbContext.Discounts
            .Where(d => d.RoomId == roomId && d.StartDate <= checkInDate && d.EndDate >= checkOutDate)
            .OrderByDescending(d => d.CreationDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Room>> GetRoomsWithMostRecentHighestDiscounts(int roomsCount)
    {
        DateTime currentDate = DateTime.UtcNow;
        return await _dbContext.Rooms
         .Include(r => r.Discounts)
         .Include(r => r.Hotel.City)
         .Include(r => r.Hotel.Images)
         .Include(r => r.Hotel.Location)
         .Where(r => r.Discounts.Any(d => currentDate >= d.StartDate && currentDate < d.EndDate)) // only active discounts
                                                                                                  // if multiple discounts are active for a room, select the most recently added one
         .Select(r => new
         {
             Room = r,
             Percentage = r.Discounts.OrderByDescending(d => d.CreationDate).First().Percentage

         })
         .OrderByDescending(r => r.Percentage)
         .Take(roomsCount)
         .Select(r => r.Room)
         .AsNoTracking()
         .ToListAsync();
    }
}

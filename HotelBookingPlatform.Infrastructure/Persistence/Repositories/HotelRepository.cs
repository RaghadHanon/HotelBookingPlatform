using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Enums;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using HotelBookingPlatform.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;

public class HotelRepository : IHotelRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;
    private readonly ILogger<HotelRepository> _logger;
    public HotelRepository(HotelBookingPlatformDbContext dbContext, ILogger<HotelRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<Hotel> AddHotelAsync(Hotel hotel)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Hotel> entry = await _dbContext.Hotels.AddAsync(hotel);

        return entry.Entity;
    }

    public async Task<Location> AddHotelLocationAsync(Hotel hotel, Location location)
    {
        await _dbContext.Locations.AddAsync(location);
        hotel.Location = location;

        return location;
    }

    public async Task<bool> HotelExistsAsync(Guid id)
    {
        return await _dbContext.Hotels.AnyAsync(h => h.Id == id);
    }

    public async Task<HotelImage> AddHotelImageAsync(Hotel hotel, HotelImage hotelImage)
    {
        await _dbContext.HotelImages.AddAsync(hotelImage);
        hotel.Images.Add(hotelImage);

        return hotelImage;
    }

    public Amenity AddAmenityToHotelAsync(Hotel hotel, Amenity amenity)
    {
        amenity.Hotels.Add(hotel);
        hotel.Amenities.Add(amenity);

        return amenity;
    }

    public async Task<bool> DeleteHotelAsync(Guid id)
    {
        Hotel? hotel = await _dbContext.Hotels.FindAsync(id);
        if (hotel is null)
        {
            return false;
        }

        _dbContext.Hotels.Remove(hotel);

        return true;
    }

    public async Task<Hotel?> GetHotelAsync(Guid id)
    {
        return await _dbContext.Hotels
            .Include(h => h.City)
            .Include(h => h.Images)
            .Include(h => h.Rooms)
            .Include(h => h.Location)
            .Include(r => r.Amenities)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Hotel?> GetHotelByNameAsync(string Name)
    {
        return await _dbContext.Hotels.FirstOrDefaultAsync(h => h.Name == Name);
    }

    public async Task<PaginatedResult<Amenity>> GetAmenitiesForHotelAsync(Guid id, GetAmenitiesQueryParametersDto request)
    {
        IQueryable<Amenity> query = _dbContext.Hotels
            .Where(h => h.Id == id)
            .Include(h => h.Amenities)
            .SelectMany(h => h.Amenities)
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
            query = query.Where(a => a.Name.StartsWith(searchTerm) || a.Description.StartsWith(searchTerm));
        }

        return query;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }

    public async Task<PaginatedResult<Hotel>> GetAllHotelsAsync(GetHotelsQueryParametersDto request)
    {
        IQueryable<Hotel> query = _dbContext.Hotels
            .Include(h => h.Images)
            .Include(h => h.City)
            .Include(h => h.Rooms)
            .Include(h => h.Location)
            .AsQueryable();
        query = SearchInNameOrDescription(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<Hotel> result = await query
                          .AsNoTracking()
                          .ToListAsync();

        return new PaginatedResult<Hotel>(result, paginationMetadata);
    }

    public async Task<PaginatedResult<Hotel>> SearchAndFilterHotelsAsync(HotelSearchAndFilterParametersDto request)
    {
        _logger.LogDebug(InfrastructureLogMessages.SearchAndFilterHotelsStarted, request);
        IQueryable<Hotel> query = _dbContext.Hotels
            .Include(h => h.Images)
            .Include(h => h.City)
            .Include(h => h.Reviews)
            .Include(h => h.Location)
            .Include(h => h.Rooms)
              .ThenInclude(r => r.BookingRooms)
            .AsSplitQuery()
            .AsQueryable();

        _logger.LogDebug(InfrastructureLogMessages.SearchingInNameOrDescription);
        query = SearchInNameOrDescription(query, request.SearchTerm);

        _logger.LogDebug(InfrastructureLogMessages.FilteringByRoomsAvailability);
        query = FilterByRoomsAvailability(query, request.CheckInDate, request.CheckOutDate, request.Rooms);

        _logger.LogDebug(InfrastructureLogMessages.FilteringByAdultsAndChildrenCapacity);
        query = FilterByAdultsAndChildrenCapacity(query, request.Adults, request.Children);

        _logger.LogDebug(InfrastructureLogMessages.FilteringByStarRating);
        query = FilterByStarRating(query, request.MinStarRating);

        _logger.LogDebug(InfrastructureLogMessages.FilteringByPrice);
        query = FilterByPrice(query, request.MinPrice, request.MaxPrice);

        _logger.LogDebug(InfrastructureLogMessages.FilteringByAmenities);
        query = FilterByAmenities(query, request.Amenities);

        _logger.LogDebug(InfrastructureLogMessages.FilteringByRoomTypes);
        query = FilterByRoomType(query, request.RoomTypes);

        _logger.LogDebug(InfrastructureLogMessages.ApplyingSorting);
        query = query.ApplySorting(request.SortOrder, request.GetSearchResultsSortingCriterion());

        _logger.LogDebug(InfrastructureLogMessages.GettingPaginationMetadata);
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);

        _logger.LogDebug(InfrastructureLogMessages.ApplyingPagination);
        query.ApplyPagination(request.PageNumber, request.PageSize);

        _logger.LogDebug(InfrastructureLogMessages.InvokingDatabaseAndGettingResult);
        List<Hotel> result = await query
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation(InfrastructureLogMessages.SearchAndFilterHotelsCompleted, request);
        return new PaginatedResult<Hotel>(result, paginationMetadata);
    }

    private static IQueryable<Hotel> SearchInNameOrDescription(IQueryable<Hotel> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query
                .Where(h => h.Name.StartsWith(searchQuery)
                            || (!string.IsNullOrWhiteSpace(h.Description) && h.Description.StartsWith(searchQuery)));
        }

        return query;
    }

    private static IQueryable<Hotel> FilterByRoomsAvailability(IQueryable<Hotel> query,
        DateTime checkInDate, DateTime checkOutDate, int roomsCount)
    {
        DateOnly checkInDateAsDateOnly = DateOnly.FromDateTime((DateTime)checkInDate);
        DateOnly checkOutDateAsDateOnly = DateOnly.FromDateTime((DateTime)checkOutDate);
        query = query.Where(h => h.Rooms
                                  .Count(r => !r.BookingRooms.Any(b => checkInDateAsDateOnly <= b.Booking.CheckOutDate && checkOutDateAsDateOnly >= b.Booking.CheckInDate))
                                  >= roomsCount);

        return query;
    }

    private static IQueryable<Hotel> FilterByAdultsAndChildrenCapacity(IQueryable<Hotel> query, int adults, int children)
    {
        query = query.Where(h => h.Rooms
                                  .Any(r => r.AdultsCapacity >= adults && r.ChildrenCapacity >= children));

        return query;
    }

    private static IQueryable<Hotel> FilterByStarRating(IQueryable<Hotel> query, int? minStarRating)
    {
        if (minStarRating.HasValue)
        {
            query = query.Where(h => h.StarRate >= minStarRating);
        }

        return query;
    }

    private static IQueryable<Hotel> FilterByPrice(IQueryable<Hotel> query, decimal? minPrice, decimal? maxPrice)
    {
        if (minPrice.HasValue)
        {
            query = query.Where(h => h.Rooms.Any(r => r.Price >= minPrice));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(h => h.Rooms.Any(r => r.Price <= maxPrice));
        }

        return query;
    }

    private static IQueryable<Hotel> FilterByRoomType(IQueryable<Hotel> query, List<RoomType>? roomTypes)
    {
        if (roomTypes != null && roomTypes.Count > 0)
        {
            query = query.Where(h => h.Rooms
                                      .Any(r => roomTypes.Contains(r.RoomType)));
        }

        return query;
    }

    private static IQueryable<Hotel> FilterByAmenities(IQueryable<Hotel> query, List<Guid>? amenities)
    {
        if (amenities != null && amenities.Count > 0)
        {
            query = query.Where(h => amenities.All(a => h.Amenities.Any(ha => ha.Id == a)));
        }

        return query;
    }
}

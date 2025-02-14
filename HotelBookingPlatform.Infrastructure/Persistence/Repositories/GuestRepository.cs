using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Domain.Models;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions;
using HotelBookingPlatform.Infrastructure.Persistence.Extensions.Sorting;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingPlatform.Infrastructure.Persistence.Repositories;
public class GuestRepository : IGuestRepository
{
    private readonly HotelBookingPlatformDbContext _dbContext;

    public GuestRepository(HotelBookingPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guest> AddGuestAsync(Guest guest)
    {
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Guest> entry = await _dbContext.Guests.AddAsync(guest);

        return entry.Entity;
    }

    public async Task<PaginatedResult<Booking>> GetAllBookingsForGuestAsync(Guid guestId, GetBookingsQueryParametersDto request)
    {
        IQueryable<Booking> query = _dbContext.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Hotel)
               .ThenInclude(h => h.City)
            .Include(b => b.BookingRooms)
               .ThenInclude(br => br.Room)
            .Where(b => b.GuestId == guestId)
            .AsQueryable();
        query = SearchInHotelNameOrDescription(query, request.SearchTerm);
        query = query.ApplySorting(request.SortOrder, request.GetSortingCriterion());
        PaginationMetadata paginationMetadata = await query.GetPaginationMetadataAsync(request.PageNumber, request.PageSize);
        query = query.ApplyPagination(request.PageNumber, request.PageSize);
        List<Booking> result = await query.ToListAsync();

        return new PaginatedResult<Booking>(result, paginationMetadata);
    }

    private static IQueryable<Booking> SearchInHotelNameOrDescription(IQueryable<Booking> query, string? searchQuery)
    {
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query
                .Where(b => b.Hotel.Name.StartsWith(searchQuery)
                            || (!string.IsNullOrWhiteSpace(b.Hotel.Description) && b.Hotel.Description.StartsWith(searchQuery)));
        }

        return query;
    }

    public async Task<Guest?> GetGuestAsync(Guid guestId)
    {
        return await _dbContext.Guests
                               .Include(g => g.Bookings)
                               .Where(g => g.Id == guestId)
                               .FirstOrDefaultAsync();
    }

    public async Task<Guest?> GetGuestByUserIdAsync(string userId)
    {
        return await _dbContext.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Guest)
                  .ThenInclude(g => g.Bookings)
                .Select(u => u.Guest)
                .FirstOrDefaultAsync();
    }

    public async Task<Guid?> GetGuestIdByUserIdAsync(string userId)
    {
        return await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Guest != null ? u.Guest.Id : Guid.NewGuid())
                .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Booking>> GetRecentBookingsInDifferentHotelsAsync(Guid guestId, int count = 5)
    {
        List<Booking> guestSortedBookings = await _dbContext.Bookings
            .Where(b => b.GuestId == guestId)
            .Include(b => b.Hotel)
              .ThenInclude(h => h.Location)
            .Include(b => b.Hotel.City)
            .Include(b => b.Hotel.Images)
            .OrderByDescending(b => b.CheckInDate)
            .AsNoTracking()
            .ToListAsync();

        IEnumerable<Booking> guestRecentBookingsInDifferentHotels = guestSortedBookings
            .GroupBy(b => b.HotelId)
            .Select(BookingsGroupPerHotel => BookingsGroupPerHotel.First())
            .Take(count);

        return guestRecentBookingsInDifferentHotels;
    }

    public async Task<bool> GuestExistsAsync(Guid guestId)
    {
        return await _dbContext.Guests.AnyAsync(g => g.Id == guestId);
    }

    public async Task<bool> HasGuestBookedHotelAsync(Hotel hotel, Guest guest)
    {
        return await _dbContext.Bookings
            .AnyAsync(b => b.GuestId == guest.Id && b.HotelId == hotel.Id);
    }

    public Task<bool> HasGuestReviewedHotelAsync(Hotel hotel, Guest guest)
    {
        return _dbContext.Reviews
            .AnyAsync(r => r.GuestId == guest.Id && r.HotelId == hotel.Id);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync() >= 1;
    }
}

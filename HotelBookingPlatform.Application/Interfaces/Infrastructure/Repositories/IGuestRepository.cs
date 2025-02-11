using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;

public interface IGuestRepository
{
    Task<Guest> AddGuestAsync(Guest guest);
    Task<PaginatedResult<Booking>> GetAllBookingsForGuestAsync(Guid guestId, GetBookingsQueryParametersDto request);
    Task<Guest?> GetGuestAsync(Guid guestId);
    Task<Guest?> GetGuestByUserIdAsync(string userId);
    Task<Guid?> GetGuestIdByUserIdAsync(string userId);
    Task<IEnumerable<Booking>> GetRecentBookingsInDifferentHotelsAsync(Guid guestId, int count = 5);
    Task<bool> GuestExistsAsync(Guid guestId);
    Task<bool> HasGuestBookedHotelAsync(Hotel hotel, Guest guest);
    Task<bool> HasGuestReviewedHotelAsync(Hotel hotel, Guest guest);
    Task<bool> SaveChangesAsync();
}

using HotelBookingPlatform.Application.DTOs.Booking;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Guest;
using HotelBookingPlatform.Application.DTOs.Hotel;

namespace HotelBookingPlatform.Application.Interfaces.Services;

public interface IGuestService
{
    Task<GuestOutputDto?> GetGuestAsync(Guid guestId);
    Task<PaginatedResult<BookingForGuestOutputDto>> GetAllBookingsForGuestAsync(Guid guestId, GetBookingsQueryParametersDto request);
    Task<IEnumerable<RecentlyVisitedHotelOutputDto>> GetRecentlyVisitedHotelsAsync(Guid guestId, int count = 5);
    Task<IEnumerable<RecentlyVisitedHotelOutputDto>> GetRecentlyVisitedHotelsForCurrentUserAsync(int count = 5);
}

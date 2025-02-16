using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task<bool> DeleteBookingAsync(Guid id);
    Task<Booking?> GetBookingAsync(Guid bookingId);
    Task<bool> SaveChangesAsync();
}
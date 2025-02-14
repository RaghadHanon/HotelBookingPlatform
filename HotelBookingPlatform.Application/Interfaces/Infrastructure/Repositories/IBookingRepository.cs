using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;

public interface IBookingRepository
{
    Task<Booking> AddBookingAsync(Booking booking);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task<bool> DeleteBookingAsync(Guid id);
    Task<Booking?> GetBookingAsync(Guid bookingId);
    Task RollbackTransactionAsync();
    Task<bool> SaveChangesAsync();
}